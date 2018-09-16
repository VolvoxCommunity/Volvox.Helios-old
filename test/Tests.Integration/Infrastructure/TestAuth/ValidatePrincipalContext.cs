using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Tests.Integration.TestAuth
{
    public class ValidatePrincipalContext : PrincipalContext<BasicAuthOptions>
    {
        public string Username { get; }
        public string Password { get; }

        public ValidatePrincipalContext(HttpContext context, AuthenticationScheme scheme, BasicAuthOptions options, string username, string password) : base (
            context,
            scheme,
            options,
            null)
        {
            Username = username;
            Password = password;
        }
        public string AuthenticationFailMessage { get; set; } = "Authentication failed.";
    }
}