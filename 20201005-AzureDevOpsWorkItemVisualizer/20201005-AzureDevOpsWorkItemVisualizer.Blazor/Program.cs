using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace _20201005_AzureDevOpsWorkItemVisualizer.Blazor
{
   public class Program
   {
      public static async Task Main(string[] args)
      {
         var builder = WebAssemblyHostBuilder.CreateDefault(args);
         builder.RootComponents.Add<App>("app");

         builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

         builder.Services.AddSingleton<AzureDevOpsClient>();
         builder.Services.AddSingleton<AzureDevOpsClientOptions>(new AzureDevOpsClientOptions
         {
         });
         builder.Services.AddSingleton<GraphGenerator>();

         await builder.Build().RunAsync();
      }
   }
}
