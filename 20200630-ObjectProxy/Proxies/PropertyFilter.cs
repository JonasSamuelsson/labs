using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Proxies
{
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

            if (attributes.Any(x => x.GetType().Name.IndexOf("ignore", StringComparison.OrdinalIgnoreCase) != -1))
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