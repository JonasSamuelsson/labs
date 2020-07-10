using Castle.Components.DictionaryAdapter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Proxies
{
   public static class Proxy
   {
      private static readonly DictionaryAdapterFactory Factory = new DictionaryAdapterFactory();
      private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object>>> InitializerCache = new ConcurrentDictionary<Type, Dictionary<string, Func<object>>>();
      private static readonly ConcurrentDictionary<Type, object> InitializedTypes = new ConcurrentDictionary<Type, object>();

      public static T NewObject<T>() => NewObject<T>(_ => { });

      public static T NewObject<T>(Action<T> action) => NewObject(new ExpandoObject(), action);

      public static T NewObject<T>(IDictionary<string, object> target) => NewObject<T>(target, _ => { });

      public static T NewObject<T>(IDictionary<string, object> target, Action<T> action)
      {
         var proxy = Wrap<T>(target);

         action(proxy);

         return proxy;
      }

      public static IProxyList<T> NewList<T>() => NewList<T>(new List<ExpandoObject>());

      public static IProxyList<T> NewList<T>(IList<ExpandoObject> expandos) => new ProxyList<T>(expandos);

      internal static T Wrap<T>(IDictionary<string, object> target)
      {
         PropertyFilter.AddPropertiesToIgnore<T>(target);
         MakeSureTypeIsInitialized<T>();

         return Factory.GetAdapter<T, object>(target);
      }

      private static void MakeSureTypeIsInitialized<T>()
      {
         var type = typeof(T);

         if (InitializedTypes.ContainsKey(type))
            return;

         lock (InitializedTypes)
         {
            if (InitializedTypes.ContainsKey(type))
               return;

            var meta = Factory.GetAdapterMeta(type);

            foreach (var property in meta.Properties)
            {
               property.Value.AddBehavior(new ObjectProxyPropertyBehavior());
            }

            InitializedTypes.TryAdd(type, null);
         }
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