﻿using _20201005_AzureDevOpsWorkItemVisualizer.Model;
using Handyman.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _20201005_AzureDevOpsWorkItemVisualizer
{
   public class AzureDevOpsClient : IAzureDevOpsClient
   {
      private IAzureDevOpsHttpClient _httpClient;
      private readonly Dictionary<int, DevOpsItem> _itemCache;

      public AzureDevOpsClient(AzureDevOpsClientOptions options) : this(new AzureDevOpsHttpClient(options))
      {
      }

      public AzureDevOpsClient(IAzureDevOpsHttpClient httpClient)
      {
         _httpClient = httpClient;
         _itemCache = new Dictionary<int, DevOpsItem>();
      }

      public async Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinished)
      {
         var result = new WorkItemCollection();

         var linkCandidates = new List<LinkCandidate>();

         foreach (var item in await GetWorkItems(workItemIds))
         {
            if (TryCreateWorkItem(item, workItemTypes, includeFinished, out var workItem) == false)
               continue;

            result.WorkItems.Add(workItem);

            foreach (var relation in item.Relations)
            {
               if (Link.TryCreate(item.Id, relation.TargetWorkItemId, relation.Attributes.Name, out var link) == false)
                  continue;

               linkCandidates.Add(new LinkCandidate
               {
                  Link = link,
                  TargetWorkItemId = relation.TargetWorkItemId
               });
            }
         }

         var ids = linkCandidates
            .Select(x => x.TargetWorkItemId)
            .ToSet();

         var items = (await GetWorkItems(ids)).ToDictionary(x => x.Id);

         foreach (var candidate in linkCandidates)
         {
            var item = items[candidate.TargetWorkItemId];

            if (TryCreateWorkItem(item, workItemTypes, includeFinished, out _) == false)
               continue;

            result.Links.Add(candidate.Link);
         }

         return result;
      }

      private static bool TryCreateWorkItem(DevOpsItem item, ISet<WorkItemType> workItemTypes, bool includeFinished, out WorkItem workItem)
      {
         if (WorkItem.TryCreate(item, out workItem) == false)
            return false;

         if (workItemTypes.Contains(workItem.Type) == false)
            return false;

         if (workItem.IsFinished && includeFinished == false)
            return false;

         return true;
      }

      private async Task<IEnumerable<DevOpsItem>> GetWorkItems(ISet<int> workItemIds)
      {
         var cached = _itemCache.Values
            .Where(x => workItemIds.Contains(x.Id))
            .ToList();

         var ids = workItemIds
            .Except(cached.Select(x => x.Id))
            .ToList();

         return (await _httpClient.GetWorkItems(ids)).Concat(cached)
            .Visit(x => _itemCache[x.Id] = x)
            .ToList();
      }

      public class DevOpsItem
      {
         public int Id { get; set; }
         public DevOpsItemFields Fields { get; set; }
         public DevOpsRelation[] Relations { get; set; }
      }

      public class DevOpsItemFields
      {
         [JsonProperty("System.State")]
         public string State { get; set; }

         [JsonProperty("System.Tags")]
         public string Tags { get; set; }

         [JsonProperty("System.Title")]
         public string Title { get; set; }

         [JsonProperty("System.WorkItemType")]
         public string Type { get; set; }
      }

      public class DevOpsRelation
      {
         public string Rel { get; set; }
         public string Url { get; set; }
         public DevOpsRelationAttributes Attributes { get; set; }

         public int TargetWorkItemId => int.TryParse(Url.Split('/').Last(), out var i) ? i : 0;
      }

      public class DevOpsRelationAttributes
      {
         public string Name { get; set; }
      }

      private class LinkCandidate
      {
         public Link Link { get; set; }
         public int TargetWorkItemId { get; set; }
      }
   }
}