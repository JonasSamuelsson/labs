using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AspNetCore.ServiceBusOutbox
{
   public static class OutboxServiceCollectionExtensions
   {
      public static void AddOutbox(this IServiceCollection services)
      {
         services.AddTransient<IOutbox, Outbox>();
         services.AddScoped<OutboxContext>();
      }
   }

   public interface IOutbox
   {
      void AddMessage(Action<IMessageBuilder> action);
      Task PublishMessages();
   }

   internal class Outbox : IOutbox
   {
      private readonly OutboxContext _outboxContext;
      private readonly IStorage _storage;
      private readonly IMessageSenderProvider _messageSenderProvider;

      public Outbox(OutboxContext outboxContext, IStorage storage, IMessageSenderProvider messageSenderProvider)
      {
         _outboxContext = outboxContext;
         _storage = storage;
         _messageSenderProvider = messageSenderProvider;
      }

      public void AddMessage(Action<IMessageBuilder> action)
      {
         var builder = new MessageBuilder();
         action.Invoke(builder);
         var message = builder.BuildMessage();
         _outboxContext.Messages.Add(message);
         _storage.AddMessage(message);
      }

      public async Task PublishMessages()
      {
         var messages = await _storage.GetMessages();

         var tasks = new List<Task>();
         foreach (var g in messages.GroupBy(x => new { x.Namespace, x.EntityPath }))
         {
            tasks.Add(PublishMessages(g.Key.Namespace, g.Key.EntityPath, g.Select(x => x.ToMessage()).ToList()));
         }

         await Task.WhenAll(tasks);
      }

      private async Task PublishMessages(string @namespace, string entityPath, IList<Message> messages)
      {
         var messageSender = _messageSenderProvider.GetMessageSender(@namespace, entityPath);
         await messageSender.SendAsync(messages);

      }
   }

   public class MessageEntity
   {
      public string Id { get; set; }
      public string MetaData { get; set; }
      public byte[] Body { get; set; }
   }

   internal class OutboxContext
   {
      public List<ServiceBusMessage> Messages { get; } = new List<ServiceBusMessage>();
   }

   public interface IMessageBuilder
   {
      IMessageBuilder Body<T>(T body) where T : class;
      IMessageBuilder Body(byte[] body, string contentType);
      IMessageBuilder CorrelationId(string correlationId);
      IMessageBuilder EntityPath(string entityPath);
      IMessageBuilder MessageId(string messageId);
      IMessageBuilder Namespace(string @namespace);
      IMessageBuilder PartitionKey(string partitionKey);
      IMessageBuilder ScheduledEnqueueTimeUtc(DateTime dateTime);
   }

   internal class MessageBuilder : IMessageBuilder
   {
      private readonly Message _message = new Message { MessageId = Guid.NewGuid().ToString() };

      public IMessageBuilder Body<T>(T body) where T : class
      {
         var json = JsonConvert.SerializeObject(body);
         var bytes = Encoding.UTF8.GetBytes(json);
         return Body(bytes, "application/json; charset=utf8");
      }

      public IMessageBuilder Body(byte[] body, string contentType)
      {
         _message.Body = body;
         _message.ContentType = contentType;
         return this;
      }

      public IMessageBuilder CorrelationId(string correlationId)
      {
         _message.CorrelationId = correlationId;
         return this;
      }

      public IMessageBuilder MessageId(string messageId)
      {
         _message.MessageId = messageId;
         return this;
      }

      public IMessageBuilder PartitionKey(string partitionKey)
      {
         throw new NotImplementedException();
      }

      public IMessageBuilder ScheduledEnqueueTimeUtc(DateTime dateTime)
      {
         _message.ScheduledEnqueueTimeUtc = dateTime;
         return this;
      }

      public ServiceBusMessage BuildMessage()
      {
         throw new NotImplementedException();
      }
   }

   public interface IMessageSenderProvider
   {
      IMessageSender GetMessageSender(string @namespace, string entityPath);
   }

   public interface IStorage
   {
      void AddMessage(MessageEntity message);
      Task<IReadOnlyCollection<MessageEntity>> GetMessages();
      Task DeleteMessages(IEnumerable<MessageEntity> messages);
   }

   internal class ServiceBusMessage
   {
      public string EntityPath { get; set; }
      public string Namespace { get; set; }
   }

   internal class OutboxMiddleware : IMiddleware
   {
      private readonly OutboxContext _outboxContext;
      private readonly IStorage _storage;
      private readonly IMessageSenderProvider _messageSenderProvider;

      public OutboxMiddleware(OutboxContext outboxContext, IStorage storage, IMessageSenderProvider messageSenderProvider)
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

   internal static class MessageConverter
   {
      internal static ServiceBusMessage EntityToMessage(MessageEntity entity)
      {
         throw new NotImplementedException();
      }

      internal static MessageEntity MessageToEntity(ServiceBusMessage message)
      {
         var metadata = new Dictionary<string, object>();
         throw new NotImplementedException();
      }
   }
}
