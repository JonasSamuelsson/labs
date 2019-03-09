using Microsoft.AspNetCore.Http;

namespace AspNetCore.ApiVersioning
{
   public class QueryStringApiVersionProvider : IApiVersionProvider
   {
      public string GetApiVersion(HttpRequest request) => request.Query["api-version"];
   }
}