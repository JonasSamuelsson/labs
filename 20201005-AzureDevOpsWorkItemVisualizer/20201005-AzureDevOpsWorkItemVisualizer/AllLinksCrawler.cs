﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _20201005_AzureDevOpsWorkItemVisualizer.Model;
using Handyman.Extensions;

namespace _20201005_AzureDevOpsWorkItemVisualizer
{
   public class AllLinksCrawler : ICrawler
   {
      private readonly IAzureDevOpsClient _client;

      public AllLinksCrawler(IAzureDevOpsClient client)
      {
         _client = client;
      }

      public async Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinishedWorkItems)
      {
         var result = await _client.GetData(workItemIds, workItemTypes, includeFinishedWorkItems);

         while (true)
         {
            var ids = result.WorkItems.Select(x => x.Id).ToList();

            workItemIds = result.Links
               .SelectMany(x => new[] { x.FromWorkItemId, x.ToWorkItemId })
               .Where(x => ids.Contains(x) == false)
               .ToSet();

            if (workItemIds.Any() == false)
               break;

            var response = await _client.GetData(workItemIds, workItemTypes, includeFinishedWorkItems);

            response.Links.ForEach(x => result.Links.Add(x));
            response.WorkItems.ForEach(x => result.WorkItems.Add(x));
         }

         return result;
      }
   }
}