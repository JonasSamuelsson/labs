﻿using _20201005_AzureDevOpsWorkItemVisualizer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _20201005_AzureDevOpsWorkItemVisualizer.Console
{
   public static class Program
   {
      public static async Task Main()
      {
         var options = new AzureDevOpsClientOptions
         {
         };

         var workItemIds = new HashSet<int> { 28652 };

         var includeWorkItemTypes = new HashSet<WorkItemType>
         {
            WorkItemType.Epic,
            WorkItemType.Feature,
            WorkItemType.PBI
         };

         var client = new AzureDevOpsClient(options);
         var crawler = new AllLinksCrawler(client);
         var data = await crawler.GetData(workItemIds, includeWorkItemTypes, includeFinishedWorkItems: false);

         var graph = new GraphGenerator().GenerateGraph(data, workItemIds);

         // use http://magjac.com/graphviz-visual-editor/ to test graph

         System.Console.WriteLine(graph);
      }
   }
}
