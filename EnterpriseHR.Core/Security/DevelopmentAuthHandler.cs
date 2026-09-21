using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace EnterpriseHR.Core.Security {
    public class DevelopmentAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>{
        public const string SchemeName = "Development";

        public DevelopmentAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
            //    var claims = new[] {
            //    new Claim(ClaimTypes.NameIdentifier, "dev-user-001"),
            //    new Claim(ClaimTypes.Email, "employee@enterprisehr.local"),
            //    new Claim(ClaimTypes.Name, "Development Employee")
            //};

            //    var identity = new ClaimsIdentity(claims, SchemeName);
            //    var principal = new ClaimsPrincipal(identity);
            //    var ticket = new AuthenticationTicket(principal, SchemeName);

            //    return Task.FromResult(AuthenticateResult.Success(ticket));

            var requestedUser = Request.Headers["X-Dev-User"].FirstOrDefault();

            var userId = requestedUser == "user2" ? "dev-user-002" : "dev-user-001";
            var email = requestedUser == "user2" ? "employee2@enterprisehr.local" : "employee1@enterprisehr.local";
            var displayName = requestedUser == "user2" ? "Development Employee 2" : "Development Employee 1";

            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, displayName)
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
