using System.Collections.Generic;
using System.Threading.Tasks;

namespace _20201005_AzureDevOpsWorkItemVisualizer
{
   public interface IAzureDevOpsHttpClient
   {
      Task<IEnumerable<T>> GetWorkItems<T>(IEnumerable<int> ids);
   }
}