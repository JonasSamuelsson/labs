using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Routing.EndpointSelection
{
   public class CustomEndpointSelector : MatcherPolicy, IEndpointSelectorPolicy
   {
      public override int Order { get; } = int.MinValue;

      public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
      {
         return endpoints.Any(x => x.Metadata.GetMetadata<VersionAttribute>() != null);
      }

      public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
      {
         string version = httpContext.Request.Query["version"];

         for (var i = 0; i < candidates.Count; i++)
         {
            var candidate = candidates[i];
            var endpoint = candidate.Endpoint;
            var attribute = endpoint.Metadata.GetMetadata<VersionAttribute>();
            var valid = attribute?.Version == version;
            candidates.SetValidity(i, valid);
         }

         return Task.CompletedTask;
      }
   }
}