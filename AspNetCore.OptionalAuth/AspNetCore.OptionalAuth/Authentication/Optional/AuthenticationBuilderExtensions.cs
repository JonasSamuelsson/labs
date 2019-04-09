using Microsoft.AspNetCore.Authentication;

namespace AspNetCore.OptionalAuth.Authentication.Optional
{
   internal static class AuthenticationBuilderExtensions
   {
      public static AuthenticationBuilder AddOptional(this AuthenticationBuilder builder)
      {
         return builder.AddScheme<OptionalAuthenticationOptions, OptionalAuthenticationHandler>(OptionalAuthenticationDefaults.AuthenticationScheme, delegate { });
      }
   }
}