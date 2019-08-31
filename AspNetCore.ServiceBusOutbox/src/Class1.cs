using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.ServiceBusOutbox
{
   public interface IOutbox
   {
      void AddMessage(Action<IServiceBusMessageBuilder> action);
   }

   internal class Outbox : IOutbox
   {
      private readonly OutboxContext _outboxContext;
      private readonly IPersistedStorage _storage;

      public Outbox(OutboxContext outboxContext, IPersistedStorage storage)
      {
         _outboxContext = outboxContext;
         _storage = storage;
      }

      public void AddMessage(Action<IServiceBusMessageBuilder> action)
      {
         var builder = new ServiceBusMessageBuilder();
         action.Invoke(builder);
         var message = builder.BuildMessage();
         _outboxContext.Messages.Add(message);
         _storage.AddMessage(message);
      }
   }

   internal class OutboxContext
   {
      public List<ServiceBusMessage> Messages { get; } = new List<ServiceBusMessage>();
   }

   public interface IServiceBusMessageBuilder
   {
      IServiceBusMessageBuilder Body<T>(T body) where T : class;
      IServiceBusMessageBuilder Body(byte[] body, string contentType);
      IServiceBusMessageBuilder Id(string id);
      IServiceBusMessageBuilder PartitionKey(string partitionKey);
   }

   internal class ServiceBusMessageBuilder : IServiceBusMessageBuilder
   {
      private readonly Message _message = new Message { MessageId = Guid.NewGuid().ToString() };

      public IServiceBusMessageBuilder Body<T>(T body) where T : class
      {
         var json = JsonConvert.SerializeObject(body);
         var bytes = Encoding.UTF8.GetBytes(json);
         return Body(bytes, "application/json; charset=utf8");
      }

      public IServiceBusMessageBuilder Body(byte[] body, string contentType)
      {
         _message.Body = body;
         _message.ContentType = contentType;
         return this;
      }

      public IServiceBusMessageBuilder Id(string id)
      {
         _message.MessageId = id;
         return this;
      }

      public IServiceBusMessageBuilder PartitionKey(string partitionKey)
      {
         throw new NotImplementedException();
      }

      public ServiceBusMessage BuildMessage()
      {
         throw new NotImplementedException();
      }
   }

   public interface IServiceBusConnectionStringProvider
   {
      string GetConnectionString(string @namespace);
   }

   public interface IMessageSenderProvider
   {
      IMessageSender GetMessageSender(string @namespace, string entityPath);
   }

   public interface IPersistedStorage
   {
      void AddMessage(ServiceBusMessage message);
      Task<IReadOnlyCollection<ServiceBusMessage>> GetMessages();
      Task DeleteMessages(IEnumerable<ServiceBusMessage> messages);
   }

   public class ServiceBusMessage
   {
      public Guid BatchId { get; set; }
      public string Namespace { get; set; }
      public string EntityPath { get; set; }

      // todo
      public Message ToMessage()
      {
         throw new NotImplementedException();
      }
   }

   internal class OutboxMiddleware : IMiddleware
   {
      private readonly OutboxContext _outboxContext;
      private readonly IPersistedStorage _storage;
      private readonly IMessageSenderProvider _messageSenderProvider;

      public OutboxMiddleware(OutboxContext outboxContext, IPersistedStorage storage, IMessageSenderProvider messageSenderProvider)
      {
         _outboxContext = outboxContext;
         _storage = storage;
         _messageSenderProvider = messageSenderProvider;
      }

      public async Task InvokeAsync(HttpContext context, RequestDelegate next)
      {
         await next(context).ConfigureAwait(false);

         if (context.Response.StatusCode >= 400)
            return;

         var messages = _outboxContext.Messages;

         if (!messages.Any())
            return;

         foreach (var g in messages.GroupBy(x => new { x.Namespace, x.EntityPath }))
         {
            var sender = _messageSenderProvider.GetMessageSender(g.Key.Namespace, g.Key.EntityPath);

            try
            {
               await sender.SendAsync(g.Select(x => x.ToMessage()).ToList());
            }
            catch
            {
               // noop
            }
         }

         await _storage.DeleteMessages(messages);
      }
   }
}
