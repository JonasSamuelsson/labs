using Newtonsoft.Json;
using Proxies;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace App
{
   class Program
   {
      private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
      {
         Converters = { new ObjectProxyJsonConverter(), new ProxyListJsonConverter() },
         Formatting = Formatting.Indented
      };

      static void Main()
      {
         var list = new List<ExpandoObject>();
         var list1 = new ProxyList<IFirst>(list);
         var list2 = new ProxyList<ISecond>(list);

         foreach (var name in new[] { "John Doe", "Bart Simpsons" })
         {
            list1.Add(Proxy.Create<IFirst>(x => x.Name = name));
         }

         foreach (var s in list2)
         {
            s.Message = $"Hello {s.Name}.";
         }

         Console.WriteLine(JsonConvert.SerializeObject(list1, JsonSerializerSettings));

         var expando = new ExpandoObject();

         //var first = Proxy.Create<IFirst>(expando, x => x.Name = "John Doe");

         //first.Strings.Add("item");
         //first.Trash = "fail";

         var second = Proxy.Create<ISecond>(expando);

         second.Child = Proxy.Create<IChild>();
         //second.Child.Id = 123;
         second.Message = $"Hello there.";
         //second.Message = $"Hello {first.Name}.";
         //second.Waste = "fail";

         var jsonSerializerSettings = JsonSerializerSettings;

         var json = JsonConvert.SerializeObject(expando, jsonSerializerSettings);

         var e = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSerializerSettings);

         Console.WriteLine(json);
      }
   }

   public interface IFirst
   {
      string Name { get; set; }
      IList<string> Strings { get; set; }

      [JsonIgnore]
      string Trash { get; set; }
   }

   public interface ISecond
   {
      string Name { get; }
      IChild Child { get; set; }
      string Message { get; set; }

      [JsonIgnore]
      string Waste { get; set; }
   }

   public interface IChild
   {
      int Id { get; set; }
   }
}
