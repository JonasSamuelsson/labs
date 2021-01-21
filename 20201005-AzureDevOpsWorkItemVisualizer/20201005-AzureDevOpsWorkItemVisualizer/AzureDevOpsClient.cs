using _20201005_AzureDevOpsWorkItemVisualizer.Model;
using Handyman.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace _20201005_AzureDevOpsWorkItemVisualizer
{
   public class AzureDevOpsClient
   {
      private static readonly HttpClient HttpClient = new HttpClient();

      private readonly AzureDevOpsClientOptions _options;

      public AzureDevOpsClient(AzureDevOpsClientOptions options)
      {
         _options = options;
      }

      public async Task<Data> LoadData(IEnumerable<int> workItemIds, bool includeFinishedWorkItems)
      {
         var originWorkItemIds = workItemIds.ToList();

         var data = new Data(includeFinishedWorkItems);

         var items = await LoadWorkItems(originWorkItemIds);

         foreach (var item in items)
         {
            if (data.TryAddWorkItem(item) == false)
               continue;

            foreach (var relation in item.Relations)
            {
               data.AddLink(item.Id, relation, new[] { LinkDirection.Forward, LinkDirection.Reverse });
            }
         }

         foreach (var linkDirection in new[] { LinkDirection.Forward, LinkDirection.Reverse })
         {
            while (true)
            {
               var unresolvedWorkItemIds = data.GetUnresolvedWorkItemIds(linkDirection);

               if (unresolvedWorkItemIds.Any() == false)
                  break;

               items = await LoadWorkItems(unresolvedWorkItemIds);

               foreach (var item in items)
               {
                  if (data.TryAddWorkItem(item) == false)
                     continue;

                  foreach (var relation in item.Relations)
                  {
                     data.AddLink(item.Id, relation, new[] { linkDirection });
                  }
               }
            }
         }

         data.MarkOriginWorkItems(originWorkItemIds);

         return data;
      }

      private async Task<IEnumerable<DevOpsItem>> LoadWorkItems(IEnumerable<int> ids)
      {
         return (await ids
            .Chunk(200)
            .Select(x => $"workitems?api-version=6.0&$expand=relations&ids={string.Join(",", ids)}")
            .Select(Load<DevOpsItem[]>)
            .WhenAll())
            .SelectMany(x => x);
      }

      private async Task<T> Load<T>(string uri)
      {
         var baseUri = new Uri($"https://dev.azure.com/{_options.Organization}/{_options.Project}/_apis/wit/");

         var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($".:{_options.PersonalAccessToken}"));

         var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, uri))
         {
            Headers =
            {
               Accept = { new MediaTypeWithQualityHeaderValue("application/json")},
               Authorization = new AuthenticationHeaderValue("Basic", auth)
            }
         };

         var response = await HttpClient.SendAsync(request);

         response.EnsureSuccessStatusCode();

         return (await response.Content.ReadAsAsync<Response<T>>()).Value;
      }

      public class Response<T>
      {
         public T Value { get; set; }
      }

      public class DevOpsItem
      {
         public int Id { get; set; }
         public DevOpsItemFields Fields { get; set; }
         public DevOpsRelation[] Relations { get; set; }
      }

      public class DevOpsItemFields
      {
         [JsonProperty("System.State")]
         public string State { get; set; }

         [JsonProperty("System.Tags")]
         public string Tags { get; set; }

         [JsonProperty("System.Title")]
         public string Title { get; set; }

         [JsonProperty("System.WorkItemType")]
         public string Type { get; set; }
      }

      public class DevOpsRelation
      {
         public string Rel { get; set; }
         public string Url { get; set; }
         public DevOpsRelationAttributes Attributes { get; set; }
      }

      public class DevOpsRelationAttributes
      {
         public string Name { get; set; }
      }
   }
}