using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.EtagParameterBinding
{
   public static class MvcOptionsExtensions
   {
      public static void AddEtagParameterBindingFilter(this MvcOptions options)
      {
         options.Filters.Add<EtagParameterBindingFilter>();
      }

      public static void AddEtagParameterBindingProvider(this MvcOptions options)
      {
         options.ModelBinderProviders.Insert(0, new EtagModelBinderProvider());
      }
   }
}