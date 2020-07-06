using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _20200518_AspNetCoreSyncOverAsync
{
   public class Program
   {
      public static void Main(string[] args)
      {
         CreateHostBuilder(args).Build().Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
               builder.ClearProviders();
               builder.AddConsole(options => options.TimestampFormat = "[HH:mm:ss.fff] ");
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
   }
}
