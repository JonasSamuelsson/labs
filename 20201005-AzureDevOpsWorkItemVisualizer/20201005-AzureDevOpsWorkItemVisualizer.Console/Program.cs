using System.Linq;
using System.Threading.Tasks;

namespace _20201005_AzureDevOpsWorkItemVisualizer.Console
{
   public static class Program
   {
      public static async Task Main()
      {
         System.Console.Write("Work item ids: ");
         var input = System.Console.ReadLine();

         var itemIdsToResolve = input.Split(',')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(int.Parse)
            .ToHashSet();

         var options = new AzureDevOpsClientOptions
         {
         };

         var data = await new AzureDevOpsClient(options).LoadData(itemIdsToResolve);

         var graph = new GraphGenerator().GenerateGraph(data);

         System.Console.WriteLine(graph);
      }
   }
}
