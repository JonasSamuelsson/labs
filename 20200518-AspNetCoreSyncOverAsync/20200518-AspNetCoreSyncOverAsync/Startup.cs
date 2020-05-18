using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace _20200518_AspNetCoreSyncOverAsync
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddHostedService<WorkerService>();
         services.AddControllers();
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         app.UseRouting();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });
      }
   }

   internal static class Utils
   {
      internal static string WorkingDirectory => Path.Combine(Environment.CurrentDirectory, "bin", "workdir");
      internal static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

      internal static string GetFilePath(string name) => Path.Combine(WorkingDirectory, $"{name}.txt");
      internal static string GetUrl(string action, string id) => $"http://localhost:56295/{action}/{id}";
   }

   [ApiController]
   public class Controller : ControllerBase
   {
      [HttpPost("sync-over-async")]
      public async Task SynchOverAsync(Payload payload)
      {
         var ticket = Enqueue(payload.Timeout);

         while (System.IO.File.Exists(ticket.path))
         {
            await Task.Delay(100);
         }
      }

      [HttpPost("claim-check")]
      public IActionResult Claim(Payload payload)
      {
         var ticket = Enqueue(payload.Timeout);

         Response.Headers[HeaderNames.Location] = Utils.GetUrl("claim-check", ticket.id);
         return StatusCode(StatusCodes.Status303SeeOther);
      }

      [HttpGet("claim-check/{id}")]
      public string Check(string id)
      {
         return System.IO.File.Exists(Utils.GetFilePath(id))
            ? "processing"
            : "done";
      }

      [HttpPost("webhook")]
      public void WebHook(Payload payload)
      {
         Enqueue(payload.Timeout, payload.Url);
      }

      [HttpPost("callback")]
      public void Callback() { }

      private static (string id, string path) Enqueue(int timeout, string url = null)
      {
         var id = Guid.NewGuid().ToString();
         var path = Utils.GetFilePath(id);

         var content = DateTime.Now.AddSeconds(timeout).ToString(Utils.DateTimeFormat);

         if (url != null)
         {
            content += $";{url}";
         }

         System.IO.File.WriteAllText(path, content);

         return (id, path);
      }

      public class Payload
      {
         public int Timeout { get; set; }
         public string Url { get; set; }
      }
   }

   public class WorkerService : IHostedService, IDisposable
   {
      private Timer _timer;

      public Task StartAsync(CancellationToken cancellationToken)
      {
         if (!Directory.Exists(Utils.WorkingDirectory))
         {
            Directory.CreateDirectory(Utils.WorkingDirectory);
         }

         _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100));

         return Task.CompletedTask;
      }

      private static void DoWork(object? state)
      {
         foreach (var path in Directory.GetFiles(Utils.WorkingDirectory))
         {
            try
            {
               var content = File.ReadAllText(path);
               var strings = content.Split(';');
               var timestamp = strings[0];

               if (DateTime.Now < DateTime.ParseExact(timestamp, Utils.DateTimeFormat, null))
                  continue;

               File.Delete(path);

               if (strings.Length > 1)
               {
                  new HttpClient().SendAsync(new HttpRequestMessage(HttpMethod.Post, strings[1]));
               }
            }
            catch { /* ignored */ }
         }
      }

      public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

      public void Dispose()
      {
         _timer?.Dispose();
      }
   }
}
