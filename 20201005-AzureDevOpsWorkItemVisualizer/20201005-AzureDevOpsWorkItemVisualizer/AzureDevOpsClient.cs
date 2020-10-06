using _20201005_AzureDevOpsWorkItemVisualizer.Model;
using Handyman.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

      public async Task<Data> LoadData(ISet<int> workItemIds)
      {
         var data = new Data();

         while (workItemIds.Any())
         {
            var items = await LoadWorkItems(workItemIds);

            foreach (var item in items)
            {
               if (Data.IsSupportedWorkItemType(item.Fields.Type, out var workItemType))
               {
                  data.Items.Add(new WorkItem
                  {
                     Id = item.Id,
                     IsDone = item.Fields.State == "Done",
                     Name = item.Fields.Title,
                     State = item.Fields.State,
                     Type = workItemType
                  });
               }
               else
               {
                  data.IgnoredWorkItemIds.Add(item.Id);
                  data.Links.RemoveWhere(x => x.From == item.Id || x.To == item.Id);
                  continue;
               }

               foreach (var relation in item.Relations)
               {
                  if (Data.IsSupportedForwardRelation(relation.Attributes.Name, out var forwardRelationAlias))
                  {
                     var toId = relation.GetToId();

                     if (data.IgnoredWorkItemIds.Contains(toId))
                        continue;

                     workItemIds.Add(toId);

                     data.Links.Add(new Link
                     {
                        From = item.Id,
                        To = toId,
                        Type = forwardRelationAlias
                     });
                  }
                  else if (Data.IsSupportedReverseRelation(relation.Attributes.Name, out var reverseRelationAlias))
                  {
                     var toId = relation.GetToId();

                     if (data.IgnoredWorkItemIds.Contains(toId))
                        continue;

                     workItemIds.Add(toId);

                     data.Links.Add(new Link
                     {
                        From = toId,
                        To = item.Id,
                        Type = reverseRelationAlias
                     });
                  }
               }
            }

            data.IgnoredWorkItemIds.ToList().ForEach(id => data.Links.RemoveWhere(x => x.From == id || x.To == id));

            var resolvedItemIds = data.Items.Select(x => x.Id).ToList();
            workItemIds.ExceptWith(resolvedItemIds);
            workItemIds.ExceptWith(data.IgnoredWorkItemIds);
         }

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

         var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.Username}:{_options.PersonalAccessToken}"));

         var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, uri))
         {
            Headers =
            {
               Accept = { new MediaTypeWithQualityHeaderValue("application/json")},
               Authorization = new AuthenticationHeaderValue("Basic", auth)
            }
         };

         var response = await HttpClient.SendAsync(request);

         if (response.StatusCode != HttpStatusCode.OK)
         {
            var s = await response.Content.ReadAsStringAsync();
         }

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

         public int GetToId() => int.Parse(Url.Split('/').Last());
      }

      public class DevOpsRelationAttributes
      {
         public string Name { get; set; }
      }
   }
}