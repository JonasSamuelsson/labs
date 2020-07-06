using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Proxies
{
   public class ProxyList
   {
      internal IList<ExpandoObject> Expandos { get; set; }
   }

   public class ProxyList<T> : ProxyList, IList<T>
   {
      public ProxyList()
      {
      }

      public ProxyList(IList<ExpandoObject> expandos)
      {
         Expandos = expandos;
      }

      public IEnumerator<T> GetEnumerator()
      {
         return Expandos.Select(Proxy.Create<T>).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public void Add(T item)
      {
         Expandos.Add(Proxy.Unwrap(item));
      }

      public void Clear()
      {
         Expandos.Clear();
      }

      public bool Contains(T item)
      {
         return Expandos.Contains(Proxy.Unwrap(item));
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
         this.ToList().CopyTo(array, arrayIndex);
      }

      public bool Remove(T item)
      {
         return Expandos.Remove(Proxy.Unwrap(item));
      }

      public int Count => Expandos.Count;
      public bool IsReadOnly => Expandos.IsReadOnly;

      public int IndexOf(T item)
      {
         return Expandos.IndexOf(Proxy.Unwrap(item));
      }

      public void Insert(int index, T item)
      {
         Expandos.Insert(index, Proxy.Unwrap(item));
      }

      public void RemoveAt(int index)
      {
         Expandos.RemoveAt(index);
      }

      public T this[int index]
      {
         get => Proxy.Wrap<T>(Expandos[index]);
         set => Expandos[index] = Proxy.Unwrap(value);
      }
   }
}