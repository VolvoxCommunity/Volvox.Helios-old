using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentCache;
using FluentCache.Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.DiscordFacing;
using Volvox.Helios.Core.Modules.DiscordFacing.Commands;
using Volvox.Helios.Core.Modules.DiscordFacing.Framework;
using Volvox.Helios.Core.Modules.StreamAnnouncer;
using Volvox.Helios.Core.Modules.StreamerRole;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Service;
using Volvox.Helios.Service.Clients;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.HostedServices.Bot;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Volvox.Helios.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // GDPR Cookie Consent
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })

                // Cookie Authentication
                .AddCookie(options =>
                {
                    options.LoginPath = "/signin";
                    options.LogoutPath = "/signout";
                })

                // Discord Authentication
                .AddDiscord(options =>
                {
                    options.ClientId = Configuration["Discord:ClientID"];
                    options.ClientSecret = Configuration["Discord:ClientSecret"];
                    options.Scope.Add("identify");
                    options.Scope.Add("email");
                    options.Scope.Add("guilds");
                    options.SaveTokens = true;
                    options.Events = new OAuthEvents
                    {
                        OnTicketReceived = context =>
                        {
                            // Add access token claim
                            var claimsIdentity = (ClaimsIdentity)context.Principal.Identity;

                            claimsIdentity.AddClaim(new Claim("access_token",
                                context.Properties.Items.FirstOrDefault(p => p.Key == ".Token.access_token").Value));

                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            context.Response.Redirect("/");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });

            // Hosted Services
            services.AddSingleton<IHostedService, BotHostedService>();

            // Settings
            services.AddSingleton<IDiscordSettings, DiscordSettings>();

            // Bot
            services.AddSingleton<IBot, Bot>();

            // Modules
            services.AddSingleton<IModule, StreamAnnouncerModule>();
            services.AddSingleton<IModule, StreamerRoleModule>();

            // Commands
            services.AddSingleton<IModule, CommandManager>();
            services.AddSingleton<ICommand, HelpCommand>();

            // All Modules
            services.AddSingleton<IList<IModule>>(s => s.GetServices<IModule>().ToList());
            services.AddSingleton<IList<ICommand>>(s => s.GetServices<ICommand>().ToList());

            // HTTP Clients
            services.AddHttpClient<DiscordAPIClient>(options =>
            {
                options.BaseAddress = new Uri("https://discordapp.com/api/");
                options.DefaultRequestHeaders.Add("Accept", "application/json");
                options.DefaultRequestHeaders.Add("User-Agent", "Volvox.Helios");
            });

            // Discord Services
            services.AddScoped<IDiscordUserGuildService, DiscordUserGuildService>();
            services.AddScoped<IDiscordGuildService, DiscordGuildService>();

            // Services
            services.AddSingleton(typeof(IModuleSettingsService<>), typeof(ModuleSettingsService<>));

            // Cache
            services.AddSingleton<ICache>(new FluentIMemoryCache(new MemoryCache(new MemoryCacheOptions())));

            // MVC
            services.AddMvc(options =>
            {
                options.Filters.Add(new ModelStateValidationFilter());
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Entity Framework
            services.AddDbContext<VolvoxHeliosContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("VolvoxHeliosDatabase")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/Error");
                app.UseStatusCodePagesWithReExecute("/Error/Errors/{0}");
                app.UseHsts();

                loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
