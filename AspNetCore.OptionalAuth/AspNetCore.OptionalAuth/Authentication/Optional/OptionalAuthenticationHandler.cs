using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AspNetCore.OptionalAuth.Authentication.Optional
{
   internal class OptionalAuthenticationHandler : AuthenticationHandler<OptionalAuthenticationOptions>
   {
      public OptionalAuthenticationHandler(IOptionsMonitor<OptionalAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
         : base(options, logger, encoder, clock)
      {
      }

      protected override Task<AuthenticateResult> HandleAuthenticateAsync()
      {
         return Task.FromResult(HandleAuthenticate());
      }

      private AuthenticateResult HandleAuthenticate()
      {
         return Request.Headers.ContainsKey(HeaderNames.Authorization)
            ? AuthenticateResult.NoResult()
            : AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), OptionalAuthenticationDefaults.AuthenticationScheme));
      }
   }
}