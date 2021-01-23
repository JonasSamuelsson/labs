using System.Collections.Generic;
using _20201005_AzureDevOpsWorkItemVisualizer.Model;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace _20201005_AzureDevOpsWorkItemVisualizer.Tests
{
   public class DirectLinksCrawlerTests
   {
      [Fact]
      public async Task ShouldCrawl()
      {
         var crawler = new DirectLinksCrawler(TestClient.Create());

         var collection = await crawler.GetData(new HashSet<int> { 2 }, new HashSet<WorkItemType> { WorkItemType.Feature }, false);

         collection.Links.ShouldContain(x => x.FromWorkItemId == 1 && x.ToWorkItemId == 2);
         collection.Links.ShouldContain(x => x.FromWorkItemId == 2 && x.ToWorkItemId == 3);
         collection.Links.ShouldContain(x => x.FromWorkItemId == 3 && x.ToWorkItemId == 4);

         collection.Links.Count.ShouldBe(3);

         collection.WorkItems.Select(x => x.Id).OrderBy(x => x).ShouldBe(new[] { 1, 2, 3, 4 });
      }
   }
}
