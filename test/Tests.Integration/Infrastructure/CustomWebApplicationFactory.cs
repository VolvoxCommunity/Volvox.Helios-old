using System;
using System.Collections.Generic;
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
using Tests.Integration.TestAuth;
using Volvox.Helios.Service;

namespace Tests.Integration.Infrastructure
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<Volvox.Helios.Web.Startup>
    {

        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.ConfigureTestServices(services =>
            {
                
                //TODO: seed in memory db with test data
                var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

                //This might kind of work, but doesn't seem to actually work. Allows access to the page, but we're still unauthed.
                //services.AddMvc(o => { o.Filters.Add(new AllowAnonymousFilter()); });

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
                                        new Claim("access_token", token)
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

                services.AddDbContext<VolvoxHeliosContext>(options =>
                {
                    options.UseInMemoryDatabase("HeliosInMemory");
                    options.UseInternalServiceProvider(serviceProvider);
                });

                //services.AddHttpClient<IDiscordAPIClient, TestDiscordAPIClient>();

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
