using Shouldly;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.RequestTimeout.Console
{
   internal class Program
   {
      private static async Task Main()
      {
         var client = new HttpClient { BaseAddress = new Uri("http://localhost:63359") };

         var value = Guid.NewGuid().ToString();

         await client.DeleteAsync("api/values");

         (await client.PostAsync($"api/values/{value}", null)).IsSuccessStatusCode.ShouldBeTrue();

         (await client.GetAsync<string[]>("api/values")).ShouldContain(value);

         await client.DeleteAsync("api/values");

         var delay = 1000;

         (await client.PostAsync($"api/values/{value}?delay={delay}", null)).StatusCode.ShouldBe(HttpStatusCode.RequestTimeout);

         await Task.Delay(delay + 100);

         (await client.GetAsync<string[]>("api/values")).ShouldNotContain(value);

         await client.DeleteAsync("api/values");
      }
   }

   internal static class Extensions
   {
      public static async Task<T> GetAsync<T>(this HttpClient client, string uri)
      {
         return await (await client.GetAsync(uri)).Content.ReadAsAsync<T>();
      }
   }
}
