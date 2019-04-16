using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;

namespace AspNetCore.EtagParameterBinding
{
   internal class EtagParameterBindingFilter : ActionFilterAttribute
   {
      public override void OnActionExecuting(ActionExecutingContext context)
      {
         TryPopulateEtagParameter(context);
         base.OnActionExecuting(context);
      }

      private void TryPopulateEtagParameter(ActionExecutingContext context)
      {
         var parameters = context.ActionDescriptor.Parameters;

         // ReSharper disable once ForCanBeConvertedToForeach
         for (int i = 0; i < parameters.Count; i++)
         {
            var parameter = parameters[i];

            if (parameter.ParameterType != typeof(string))
               continue;

            var name = parameter.Name;

            if (name.Equals("etag", StringComparison.OrdinalIgnoreCase) == false)
               continue;

            if (TryGetEtag(context.HttpContext.Request.Headers, out var etag))
               context.ActionArguments[name] = etag;

            return;
         }
      }

      private bool TryGetEtag(IHeaderDictionary headers, out string etag)
      {
         const string inm = HeaderNames.IfNoneMatch;
         const string im = HeaderNames.IfMatch;

         if (headers.TryGetValue(inm, out var values) || headers.TryGetValue(im, out values))
         {
            etag = values.ToString();
            return true;
         }

         etag = null;
         return false;
      }
   }
}