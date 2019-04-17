using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace AspNetCore.EtagParameterBinding
{
   internal class EtagModelBinder : IModelBinder
   {
      public Task BindModelAsync(ModelBindingContext bindingContext)
      {
         if (TryGetEtag(bindingContext.HttpContext.Request, out var etag))
            bindingContext.Result = ModelBindingResult.Success(etag);

         return Task.CompletedTask;
      }

      private static bool TryGetEtag(HttpRequest request, out string etag)
      {
         var headers = request.Headers;

         var header = request.Method == HttpMethods.Get
            ? HeaderNames.IfNoneMatch
            : HeaderNames.IfMatch;

         if (headers.TryGetValue(header, out var values))
         {
            etag = values.ToString();
            return true;
         }

         etag = null;
         return false;
      }
   }
}