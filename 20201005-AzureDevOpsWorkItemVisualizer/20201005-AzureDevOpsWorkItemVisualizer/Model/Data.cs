using System;
using System.Collections.Generic;

namespace _20201005_AzureDevOpsWorkItemVisualizer.Model
{
   public class Data
   {
      public static bool IsSupportedWorkItemType(string type, out WorkItemType workItemType)
      {
         if (type == "Feature")
         {
            workItemType = WorkItemType.Feature;
            return true;
         }

         if (type == "Product Backlog Item")
         {
            workItemType = WorkItemType.PBI;
            return true;
         }

         workItemType = default;
         return false;
      }

      public static bool IsSupportedForwardRelation(string type, out LinkType linkType)
      {
         if (type == "Child")
         {
            linkType = LinkType.HasChild;
            return true;
         }

         if (type == "Predecessor")
         {
            linkType = LinkType.DependsOn;
            return true;
         }

         linkType = default;
         return false;
      }

      public static bool IsSupportedReverseRelation(string type, out LinkType linkType)
      {
         if (type == "Parent")
         {
            linkType = LinkType.HasChild;
            return true;
         }

         if (type == "Successor")
         {
            linkType = LinkType.DependsOn;
            return true;
         }

         linkType = default;
         return false;
      }

      public HashSet<WorkItem> Items { get; set; } = new HashSet<WorkItem>(Comparer<WorkItem>.Create(x => x.Id));
      public HashSet<int> IgnoredWorkItemIds { get; set; } = new HashSet<int>();
      public HashSet<Link> Links { get; set; } = new HashSet<Link>(Comparer<Link>.Create(x => x.Id));

      private class Comparer<T> : IEqualityComparer<T>
      {
         public static IEqualityComparer<T> Create<TValue>(Func<T, TValue> selector)
         {
            return new Comparer<T>(x => selector.Invoke(x).GetHashCode());
         }

         private readonly Func<T, int> _selector;

         private Comparer(Func<T, int> selector)
         {
            _selector = selector;
         }

         public bool Equals(T x, T y)
         {
            return GetHashCode(x) == GetHashCode(y);
         }

         public int GetHashCode(T obj)
         {
            return _selector.Invoke(obj);
         }
      }
   }
}