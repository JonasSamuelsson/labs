﻿using System.Collections.Generic;
using System.Threading.Tasks;
using _20201005_AzureDevOpsWorkItemVisualizer.Model;

namespace _20201005_AzureDevOpsWorkItemVisualizer
{
   public interface IAzureDevOpsClient
   {
      Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinished);
   }
}