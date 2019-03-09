using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspNetCore.ApiVersioning
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection AddApiVersioning(this IServiceCollection services)
      {
         services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
         services.TryAddTransient<IApiVersionProvider, QueryStringApiVersionProvider>();
         services.TryAddTransient<IApiVersionValidationStrategy, ExactMatchApiVersionValidationStrategy>();
         //services.TryAddTransient<IApiVersionValidationStrategy, SemVerApiVersionValidationStrategy>();

         return services;
      }
   }
}