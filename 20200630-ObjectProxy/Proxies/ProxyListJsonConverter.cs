using System;
using Newtonsoft.Json;

namespace Proxies
{
   public class ProxyListJsonConverter : JsonConverter
   {
      public override bool CanRead => false;

      public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
      {
         serializer.Serialize(writer, (value as ProxyList)?.Expandos);
      }

      public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
      {
         throw new NotImplementedException();
      }

      public override bool CanConvert(Type objectType)
      {
         return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ProxyList<>);
      }
   }
}