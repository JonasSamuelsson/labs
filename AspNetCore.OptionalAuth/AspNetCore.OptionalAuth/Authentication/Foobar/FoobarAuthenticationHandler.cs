using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AspNetCore.OptionalAuth.Authentication.Foobar
{
   internal class FoobarAuthenticationHandler : AuthenticationHandler<FoobarAuthenticationOptions>
   {
      public FoobarAuthenticationHandler(IOptionsMonitor<FoobarAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
         : base(options, logger, encoder, clock)
      {
      }

      protected override Task<AuthenticateResult> HandleAuthenticateAsync()
      {
         return Task.FromResult(HandleAuthenticate());
      }

      private AuthenticateResult HandleAuthenticate()
      {
         if (Request.Headers[HeaderNames.Authorization] == "foobar")
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), FoobarAuthenticationDefaults.AuthenticationScheme));

         return AuthenticateResult.NoResult();
      }
   }
}