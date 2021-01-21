using System.Collections.Generic;

namespace _20201005_AzureDevOpsWorkItemVisualizer.Model
{
   public class WorkItem
   {
      public int Id { get; set; }
      public bool IsDone { get; set; }
      public string Name { get; set; }
      public bool IsOrigin { get; set; }
      public string State { get; set; }
      public IEnumerable<string> Tags { get; set; }
      public WorkItemType Type { get; set; }
   }
}