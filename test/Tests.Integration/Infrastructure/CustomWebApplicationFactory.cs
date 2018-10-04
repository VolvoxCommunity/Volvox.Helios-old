using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tests.Integration.Helpers;
using Tests.Integration.Infrastructure.TestServices;
using Tests.Integration.TestAuth;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Service;
using Volvox.Helios.Service.Clients;

namespace Tests.Integration.Infrastructure
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<Volvox.Helios.Web.Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

                services.AddDbContext<VolvoxHeliosContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(serviceProvider);
                });

                services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasicAuth(
                    options =>
                    {
                        options.Realm = "Volvox Test";
                        options.Events = new BasicAuthEvents
                        {
                            OnValidatePrincipal = context =>
                            {
                                if (context.Username.Equals("testuser")
                                && context.Password.Equals("testpassword"))
                                {
                                    var tokenHelper = new DiscordClientCredTokenHelper(ConfigurationHelper.Configuration);
                                    var token = tokenHelper.GetToken().Result;
                                    var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.Name, context.Username, context.Options.ClaimsIssuer),
                                        new Claim("access_token", token),
                                    };

                                    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, BasicAuthenticationDefaults.AuthenticationScheme));
                                    context.Principal = principal;
                                }
                                else
                                {
                                    context.AuthenticationFailMessage = "Auth failed";
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });

                services.Remove(services.FirstOrDefault(d => d.ServiceType == typeof(IDiscordAPIClient)));
                services.AddHttpClient<IDiscordAPIClient, TestDiscordApiClient>();

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<VolvoxHeliosContext>();
                    var logger = scopedServices
                    .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                    db.Database.EnsureCreated();
                }
            });
            base.ConfigureWebHost(builder);
        }
    }
}
