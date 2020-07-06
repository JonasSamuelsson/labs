using Castle.Components.DictionaryAdapter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Proxies
{
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
            property.Value.AddBehavior(new ObjectProxyPropertyBehavior());
         }

         var proxy = Wrap<T>(target);

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

      internal static T Wrap<T>(IDictionary<string, object> target)
      {
         return Factory.GetAdapter<T, object>(target);
      }

      internal static bool TryUnwrap(object item, out ExpandoObject expando)
      {
         expando = null;

         if (item is DictionaryAdapterBase dictionaryAdapterBase)
         {
            var dictionaryAdapterInstance = dictionaryAdapterBase.This;
            var dictionary = dictionaryAdapterInstance.Dictionary;
            var field = dictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
            expando = (ExpandoObject)field.GetValue(dictionary);
            return true;
         }

         return false;
      }

      public static ExpandoObject Unwrap<T>(T item)
      {
         return TryUnwrap(item, out var expando) ? expando : throw new InvalidOperationException();
      }
   }
}