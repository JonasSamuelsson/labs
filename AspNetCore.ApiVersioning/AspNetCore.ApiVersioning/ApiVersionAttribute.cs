using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.ApiVersioning
{
   public class ApiVersionAttribute : ActionFilterAttribute
   {
      private readonly IReadOnlyList<string> _apiVersions;

      public ApiVersionAttribute(string apiVersion)
         : this(new[] { apiVersion })
      {
      }

      public ApiVersionAttribute(string[] apiVersions)
      {
         _apiVersions = apiVersions;
      }

      public bool Optional { get; set; }
      public Type ValidationStrategy { get; set; }

      public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
      {
         var apiVersionReader = context.HttpContext.RequestServices.GetRequiredService<IApiVersionProvider>();
         var apiVersion = apiVersionReader.GetApiVersion(context.HttpContext.Request);

         var apiVersionValidator = ValidationStrategy != null
         ? (IApiVersionValidationStrategy)(context.HttpContext.RequestServices.GetService(ValidationStrategy) ?? Activator.CreateInstance(ValidationStrategy))
         : context.HttpContext.RequestServices.GetRequiredService<IApiVersionValidationStrategy>();

         if (apiVersionValidator.Validate(apiVersion, Optional, _apiVersions, out var matchedVersion, out var error))
         {
            context.ActionArguments["apiVersion"] = matchedVersion;
            await next().ConfigureAwait(false);
         }

         context.Result = new BadRequestObjectResult(new ProblemDetails { Detail = error });
      }
   }
}
