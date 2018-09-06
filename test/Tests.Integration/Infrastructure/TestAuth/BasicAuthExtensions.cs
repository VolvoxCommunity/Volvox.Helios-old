using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication;

namespace Tests.Integration.TestAuth
{
    public static class BasicAuthExtensions
    {
        public static AuthenticationBuilder AddBasicAuth(this AuthenticationBuilder builder)
        {
            return builder.AddBasicAuth(BasicAuthenticationDefaults.AuthenticationScheme);
        }

        public static AuthenticationBuilder AddBasicAuth(this AuthenticationBuilder builder, string authScheme)
        {
            return builder.AddBasicAuth(authScheme, null);
        }

        public static AuthenticationBuilder AddBasicAuth(this AuthenticationBuilder builder, Action<BasicAuthOptions> options)
        {
            return builder.AddBasicAuth(BasicAuthenticationDefaults.AuthenticationScheme, options);
        }

        public static AuthenticationBuilder AddBasicAuth(
            this AuthenticationBuilder builder,
            string authScheme,
            Action<BasicAuthOptions> options)
        {
            return builder.AddScheme<BasicAuthOptions, BasicAuthHandler>(authScheme, options);
        }

    }
}
