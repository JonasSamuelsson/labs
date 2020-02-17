using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;

namespace AspNetCore.EtagParameterBinding
{
   public class ETagParameterModelBinderActionDescriptorProvider : IActionDescriptorProvider
   {
      internal static readonly Dictionary<string, string> Mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
      {
         {"ifMatch", HeaderNames.IfMatch},
         {"ifMatchETag", HeaderNames.IfMatch},
         {"ifNoneMatch", HeaderNames.IfNoneMatch},
         {"ifNoneMatchETag", HeaderNames.IfNoneMatch}
      };

      public void OnProvidersExecuted(ActionDescriptorProviderContext context)
      {
         foreach (var action in context.Results)
         {
            foreach (var parameter in action.Parameters)
            {
               if (parameter.ParameterType != typeof(string))
                  continue;

               if (!Mappings.TryGetValue(parameter.Name, out var headerName))
                  continue;

               parameter.BindingInfo.BindingSource = BindingSource.Header;
               parameter.BindingInfo.BinderType = typeof(ETagParameterModelBinder);
               parameter.BindingInfo.BinderModelName = headerName;
            }
         }
      }

      public void OnProvidersExecuting(ActionDescriptorProviderContext context)
      {
      }

      public int Order { get; }
   }
}