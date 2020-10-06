namespace _20201005_AzureDevOpsWorkItemVisualizer.Model
{
   public class Link
   {
      public int From { get; set; }
      public int To { get; set; }
      public LinkType Type { get; set; }

      public string Id => $"{From}:{To}:{Type}";
   }
}