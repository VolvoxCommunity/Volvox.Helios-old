using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Tests.Integration.TestAuth
{
    public class BasicAuthHandler : AuthenticationHandler<BasicAuthOptions>
    {
        public BasicAuthHandler(IOptionsMonitor<BasicAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(
            options, logger, encoder, clock)
        {
        }

        protected new BasicAuthEvents Events
        {
            get
            {
                return (BasicAuthEvents)base.Events;
            }

            set
            {
                base.Events = value;
            }
        }

        protected override Task<object> CreateEventsAsync()
        {
            return Task.FromResult<object>(new BasicAuthEvents());
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeaderValues = this.Request.Headers.GetCommaSeparatedValues(HeaderNames.Authorization);

            if (authorizationHeaderValues == null || authorizationHeaderValues.Length == 0)
            {
                this.Logger.LogDebug("Authorization header missing from request");
                return AuthenticateResult.NoResult();
            }

            var basicAuthHeader = authorizationHeaderValues.FirstOrDefault(x => x.StartsWith("Basic "));

            if (string.IsNullOrEmpty(basicAuthHeader))
            {
                this.Logger.LogDebug("Authorization header is not of type Basic");
                return AuthenticateResult.NoResult();
            }

            var credentials = basicAuthHeader.Replace($"Basic ", string.Empty).Trim();

            if (string.IsNullOrEmpty(credentials))
            {
                return AuthenticateResult.Fail("Credentials not present");
            }

            string decodedCredentials;

            try
            {
                decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(credentials));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException || ex is DecoderFallbackException)
            {
                return AuthenticateResult.Fail("The credentials could not be base64 decoded");
            }

            var userAndPass = decodedCredentials.Split(':');

            if (userAndPass.Length < 2)
            {
                return AuthenticateResult.Fail("username or password is missing");
            }

            var context = new ValidatePrincipalContext(this.Context, this.Scheme, this.Options, userAndPass[0], userAndPass[1]);
            await this.Events.ValidatePrincipalAsync(context);

            if (context.Principal == null)
            {
                return AuthenticateResult.Fail(context.AuthenticationFailMessage);
            }

            return AuthenticateResult.Success(new AuthenticationTicket(context.Principal, context.Properties, this.Scheme.Name));
        }

        protected override Task HandleChallengeAsync(Microsoft.AspNetCore.Authentication.AuthenticationProperties context)
        {
            var realmHeader = new NameValueHeaderValue("realm", $"\"{this.Options.Realm}\"");
            this.Response.StatusCode = StatusCodes.Status401Unauthorized;
            this.Response.Headers.Append(HeaderNames.WWWAuthenticate, $"Basic {realmHeader}");
            return Task.CompletedTask;
        } 
    }
}
