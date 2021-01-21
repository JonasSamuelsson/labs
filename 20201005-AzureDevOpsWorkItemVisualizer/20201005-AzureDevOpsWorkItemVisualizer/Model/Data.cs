using System;
using System.Collections.Generic;
using System.Linq;

namespace _20201005_AzureDevOpsWorkItemVisualizer.Model
{
   public class Data
   {
      private static readonly Dictionary<string, WorkItemType> SupportedWorkItemTypes = new Dictionary<string, WorkItemType>
      {
         {"Feature", WorkItemType.Feature},
         {"Product Backlog Item", WorkItemType.PBI}
      };

      private static readonly SupportedLinkType[] SupportedLinkTypes = new[]
      {
         new SupportedLinkType
         {
            LinkDirection = LinkDirection.Forward,
            LinkType = LinkType.HasChild,
            RelationName = "Child"
         },
         new SupportedLinkType
         {
            LinkDirection = LinkDirection.Forward,
            LinkType = LinkType.DependsOn,
            RelationName = "Predecessor"
         },
         new SupportedLinkType
         {
            LinkDirection = LinkDirection.Reverse,
            LinkType = LinkType.HasChild,
            RelationName = "Parent"
         },
         new SupportedLinkType
         {
            LinkDirection = LinkDirection.Reverse,
            LinkType = LinkType.DependsOn,
            RelationName = "Successor"
         }
      };

      private readonly HashSet<int> _ignoredWorkItemIds = new HashSet<int>();
      private readonly HashSet<Link> _links = new HashSet<Link>(Comparer<Link>.Create(x => x.Id));
      private readonly HashSet<WorkItem> _workItems = new HashSet<WorkItem>(Comparer<WorkItem>.Create(x => x.Id));

      public IEnumerable<WorkItem> WorkItems => _workItems;
      public IEnumerable<Link> Links => _links;

      public void MarkOriginWorkItems(IEnumerable<int> workItemIds)
      {
         foreach (var workItem in _workItems)
         {
            workItem.IsOrigin = workItemIds.Contains(workItem.Id);
         }
      }

      public bool TryAddWorkItem(AzureDevOpsClient.DevOpsItem item)
      {
         if (SupportedWorkItemTypes.TryGetValue(item.Fields.Type, out var workItemType) == false)
         {
            _ignoredWorkItemIds.Add(item.Id);
            _links.RemoveWhere(x => x.FromWorkItemId == item.Id || x.ToWorkItemId == item.Id);
            return false;
         }

         _workItems.Add(new WorkItem
         {
            Id = item.Id,
            IsDone = item.Fields.State == "Done",
            Name = item.Fields.Title,
            IsOrigin = false, // this will be set later
            State = item.Fields.State,
            Tags = ParseWorkItemTags(item.Fields.Tags),
            Type = workItemType
         });

         return true;
      }

      private static IEnumerable<string> ParseWorkItemTags(string tags)
      {
         return tags?.Split(';').Select(x => x.Trim()).OrderBy(x => x) ?? Enumerable.Empty<string>();
      }

      public void AddLink(int workItemId, AzureDevOpsClient.DevOpsRelation relation,
         IEnumerable<LinkDirection> allowedLinkDirections)
      {
         var item = SupportedLinkTypes.FirstOrDefault(x => allowedLinkDirections.Contains(x.LinkDirection) && x.RelationName == relation.Attributes.Name);

         if (item == null)
            return;

         var toWorkItemId = item.LinkDirection == LinkDirection.Forward ? GetTargetWorkItemId(relation) : workItemId;

         if (_ignoredWorkItemIds.Contains(toWorkItemId))
            return;

         _links.Add(new Link
         {
            Type = item.LinkType,
            FromWorkItemId = item.LinkDirection == LinkDirection.Forward ? workItemId : GetTargetWorkItemId(relation),
            ToWorkItemId = toWorkItemId
         });
      }

      private static int GetTargetWorkItemId(AzureDevOpsClient.DevOpsRelation relation)
      {
         return int.Parse(relation.Url.Split('/').Last());
      }

      public IReadOnlyCollection<int> GetUnresolvedWorkItemIds(LinkDirection linkDirection)
      {
         var resolvedWorkItemIds = _workItems.Select(x => x.Id).ToList();

         return _links
            .Select(x => linkDirection == LinkDirection.Forward ? x.ToWorkItemId : x.FromWorkItemId)
            .Distinct()
            .Where(x => !resolvedWorkItemIds.Contains(x) || _ignoredWorkItemIds.Contains(x))
            .ToList();
      }

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

      private class SupportedLinkType
      {
         public LinkDirection LinkDirection { get; set; }
         public LinkType LinkType { get; set; }
         public string RelationName { get; set; }
      }
   }
}