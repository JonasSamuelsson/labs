using Castle.Components.DictionaryAdapter;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace App
{
   class Program
   {
      static void Main()
      {
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

         var jsonSerializerSettings = new JsonSerializerSettings
         {
            Converters = { new MyJsonConverter() },
            Formatting = Formatting.Indented
         };

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
      IChild Child { get; set; }
      string Message { get; set; }

      [JsonIgnore]
      string Waste { get; set; }
   }

   public interface IChild
   {
      int Id { get; set; }
   }

   internal class MyJsonConverter : JsonConverter
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

   public static class Proxy
   {
      private static readonly DictionaryAdapterFactory Factory = new DictionaryAdapterFactory();
      private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object>>> InitilaizerCache = new ConcurrentDictionary<Type, Dictionary<string, Func<object>>>();

      public static T Create<T>() => Create<T>(_ => { });

      public static T Create<T>(Action<T> action) => Create(new ExpandoObject(), action);

      public static T Create<T>(IDictionary<string, object> target) => Create<T>(target, _ => { });

      public static T Create<T>(IDictionary<string, object> target, Action<T> action)
      {
         PropertyFilter.AddPropertiesToIgnore<T>(target);

         Populate<T>(target);

         var meta = Factory.GetAdapterMeta(typeof(T));

         foreach (var property in meta.Properties)
         {
            property.Value.AddBehavior(new MyBehavior());
         }

         var proxy = Factory.GetAdapter<T, object>(target);

         action(proxy);

         return proxy;
      }

      private static void Populate<T>(IDictionary<string, object> dictionary)
      {
         var type = typeof(T);
         var initializers = InitilaizerCache.GetOrAdd(type, GetInitializers);

         foreach (var kvp in initializers)
         {
            if (dictionary.ContainsKey(kvp.Key))
               continue;

            dictionary[kvp.Key] = kvp.Value();
         }
      }

      private static Dictionary<string, Func<object>> GetInitializers(Type type)
      {
         var dictionary = new Dictionary<string, Func<object>>();

         foreach (var property in new[] { type }.Concat(type.GetInterfaces()).SelectMany(x => x.GetProperties()))
         {
            var propertyType = property.PropertyType;

            if (!propertyType.IsGenericType)
               continue;

            if (propertyType.GetGenericTypeDefinition() != typeof(IList<>))
               continue;

            var genericArgument = propertyType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(genericArgument);
            dictionary.Add(property.Name, () => Activator.CreateInstance(listType));
         }

         return dictionary;
      }

      private static IList<T> CreateList<T>() => new List<T>();
   }

   public class MyBehavior : IDictionaryBehavior, IDictionaryPropertyGetter, IDictionaryPropertySetter
   {
      public IDictionaryBehavior Copy()
      {
         return new MyBehavior();
      }

      public int ExecutionOrder => 0;

      public object GetPropertyValue(IDictionaryAdapter dictionaryAdapter, string key, object storedValue,
         PropertyDescriptor property, bool ifExists)
      {
         return storedValue;
      }

      public bool SetPropertyValue(IDictionaryAdapter dictionaryAdapter, string key, ref object value, PropertyDescriptor property)
      {
         if (value is DictionaryAdapterBase dictionaryAdapterBase)
         {
            var dictionaryAdapterInstance = dictionaryAdapterBase.This;
            var dictionary = dictionaryAdapterInstance.Dictionary;
            var field = dictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
            value = field.GetValue(dictionary);
         }

         return true;
      }
   }

   public static class PropertyFilter
   {
      private static ConcurrentDictionary<Type, HashSet<string>> Cache = new ConcurrentDictionary<Type, HashSet<string>>();
      private static readonly HashSet<string> EmptySet = new HashSet<string>();
      private static string Key = "__ignore";

      public static void AddPropertiesToIgnore<T>(IDictionary<string, object> dictionary)
      {
         var type = typeof(T);

         var properties = Cache.GetOrAdd(type, GetPropertiesToIgnore);

         lock (dictionary)
         {
            if (dictionary.TryGetValue(Key, out var o))
            {
               ((HashSet<string>)o).UnionWith(properties);
            }
            else
            {
               dictionary[Key] = properties;
            }
         }
      }

      private static HashSet<string> GetPropertiesToIgnore(Type type)
      {
         var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

         foreach (var property in new[] { type }.Concat(type.GetInterfaces()).SelectMany(x => x.GetProperties()))
         {
            var attributes = property.GetCustomAttributes(true);

            if (attributes.Any(x => x.GetType().Name.Contains("ignore", StringComparison.OrdinalIgnoreCase)))
               set.Add(property.Name);
         }

         if (set.Any())
         {
            set.Add(Key);
         }

         return set;
      }

      public static HashSet<string> GetPropertiesToIgnore(IDictionary<string, object> dictionary)
      {
         return dictionary.TryGetValue(Key, out var value) ? (HashSet<string>)value : EmptySet;
      }
   }
}
