namespace _20201005_AzureDevOpsWorkItemVisualizer.Model
{
   public class WorkItem
   {
      public int Id { get; set; }
      public bool IsDone { get; set; }
      public string Name { get; set; }
      public string State { get; set; }
      public WorkItemType Type { get; set; }
   }
}