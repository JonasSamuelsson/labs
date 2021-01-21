namespace _20201005_AzureDevOpsWorkItemVisualizer.Model
{
   public class Link
   {
      public int FromWorkItemId { get; set; }
      public int ToWorkItemId { get; set; }
      public LinkType Type { get; set; }

      public string Id => $"{FromWorkItemId}:{ToWorkItemId}:{Type}";
   }
}