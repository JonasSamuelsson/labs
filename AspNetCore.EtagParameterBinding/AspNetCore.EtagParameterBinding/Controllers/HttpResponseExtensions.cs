using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace AspNetCore.EtagParameterBinding.Controllers
{
   public static class HttpResponseExtensions
   {
      public static void SetETagHeader(this HttpResponse response, string eTag)
      {
         response.Headers[HeaderNames.ETag] = eTag;
      }
   }
}