using Microsoft.AspNetCore.Http;

namespace AspNetCore.ApiVersioning
{
   public interface IApiVersionProvider
   {
      string GetApiVersion(HttpRequest request);
   }
}