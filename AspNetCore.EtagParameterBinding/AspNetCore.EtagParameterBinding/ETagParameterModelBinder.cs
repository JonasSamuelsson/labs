using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspNetCore.EtagParameterBinding
{
   public class ETagParameterModelBinder : IModelBinder
   {
      public Task BindModelAsync(ModelBindingContext bindingContext)
      {
         var headers = bindingContext.HttpContext.Request.Headers;

         bindingContext.Result = headers.TryGetValue(bindingContext.BinderModelName, out var values)
            ? ModelBindingResult.Success(values.ToString())
            : ModelBindingResult.Failed();

         return Task.CompletedTask;
      }
   }
}