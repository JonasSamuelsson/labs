using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Proxies
{
   public class ObjectProxyJsonConverter : JsonConverter
   {
      public override bool CanRead => false;

      public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
      {
         if (value == null)
         {
            writer.WriteNull();
            return;
         }

         var dictionary = (IDictionary<string, object>)value;

         var ignore = PropertyFilter.GetPropertiesToIgnore(dictionary);

         var copy = new Dictionary<object, object>();

         foreach (var kvp in dictionary)
         {
            if (ignore.Contains(kvp.Key))
               continue;

            copy[kvp.Key] = kvp.Value;
         }

         serializer.Serialize(writer, copy);
      }

      public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
      {
         throw new NotImplementedException();
      }

      public override bool CanConvert(Type objectType)
      {
         foreach (var @interface in objectType.GetInterfaces())
         {
            if (!@interface.IsGenericType)
               continue;

            var genericTypeDefinition = @interface.GetGenericTypeDefinition();

            if (genericTypeDefinition != typeof(IDictionary<,>))
               continue;

            var genericArguments = @interface.GetGenericArguments();

            if (genericArguments[0] == typeof(string) && genericArguments[1] == typeof(object))
               return true;
         }

         return false;
      }
   }
}