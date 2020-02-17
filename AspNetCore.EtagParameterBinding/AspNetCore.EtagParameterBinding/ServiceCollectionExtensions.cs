using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.EtagParameterBinding
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection AddETagParameterBinding(this IServiceCollection services)
      {
         services.AddControllers(options => options.ModelBinderProviders.Insert(0, new ETagParameterModelBinderProvider()));
         services.AddSingleton<IActionDescriptorProvider, ETagParameterModelBinderActionDescriptorProvider>();
         services.AddSingleton<ETagParameterModelBinder>();
         return services;
      }
   }
}