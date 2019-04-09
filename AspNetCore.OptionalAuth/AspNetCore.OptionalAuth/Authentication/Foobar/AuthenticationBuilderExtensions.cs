using Microsoft.AspNetCore.Authentication;

namespace AspNetCore.OptionalAuth.Authentication.Foobar
{
   internal static class AuthenticationBuilderExtensions
   {
      public static AuthenticationBuilder AddFoobar(this AuthenticationBuilder builder)
      {
         return builder.AddScheme<FoobarAuthenticationOptions, FoobarAuthenticationHandler>(FoobarAuthenticationDefaults.AuthenticationScheme, delegate { });
      }
   }
}