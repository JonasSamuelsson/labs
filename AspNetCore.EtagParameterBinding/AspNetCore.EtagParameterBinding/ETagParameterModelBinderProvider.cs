using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.EtagParameterBinding
{
   public class ETagParameterModelBinderProvider : IModelBinderProvider
   {
      public IModelBinder GetBinder(ModelBinderProviderContext context)
      {
         return context.Services.GetRequiredService<ETagParameterModelBinder>();
      }
   }
}