using System.Reflection;
using Castle.Components.DictionaryAdapter;

namespace Proxies
{
   public class ObjectProxyPropertyBehavior : IDictionaryPropertyGetter, IDictionaryPropertySetter
   {
      public IDictionaryBehavior Copy()
      {
         return new ObjectProxyPropertyBehavior();
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
}