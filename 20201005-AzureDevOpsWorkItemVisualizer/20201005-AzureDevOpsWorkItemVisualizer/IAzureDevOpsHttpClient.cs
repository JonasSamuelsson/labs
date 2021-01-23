using System.Collections.Generic;
using System.Threading.Tasks;

namespace _20201005_AzureDevOpsWorkItemVisualizer
{
   public interface IAzureDevOpsHttpClient
   {
      Task<IEnumerable<AzureDevOpsClient.DevOpsItem>> GetWorkItems(IEnumerable<int> ids);
   }
}