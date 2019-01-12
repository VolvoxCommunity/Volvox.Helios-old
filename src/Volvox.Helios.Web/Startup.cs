using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord.WebSocket;
using FluentCache;
using FluentCache.Microsoft.Extensions.Caching.Memory;
using Hangfire;
using Hangfire.SqlServer;
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
using Newtonsoft.Json;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Core.Modules.ChatTracker;
using Volvox.Helios.Core.Modules.Command;
using Volvox.Helios.Core.Modules.Command.Commands;
using Volvox.Helios.Core.Modules.Command.Framework;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.ModerationModule;
using Volvox.Helios.Core.Modules.ModerationModule.BypassCheck;
using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Core.Modules.ModerationModule.Utils.FilterFactory;
using Volvox.Helios.Core.Modules.ModerationModule.Utils.PunishmentFactory;
using Volvox.Helios.Core.Modules.ModerationModule.ViolationService;
using Volvox.Helios.Core.Modules.ModerationModule.WarningService;
using Volvox.Helios.Core.Modules.ReminderModule;
using Volvox.Helios.Core.Modules.Streamer;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Service;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.Clients;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Discord.UserGuild;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.Jobs;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.HostedServices.Bot;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Discord.WebSocket;
using Microsoft.AspNetCore.HttpOverrides;
using Volvox.Helios.Core.Modules.Streamer;
using System.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Volvox.Helios.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
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
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<IBot, Bot>();

            // Modules
            services.AddSingleton<IModule, StreamerModule>();
            services.AddSingleton<IModule, ChatTrackerModule>();
            services.AddSingleton<IModule, RemembotModule>();
            services.AddSingleton<IModule, ModerationModule>();

            // Moderation Module specific services
            services.AddSingleton<IViolationService, ViolationService>();
            services.AddSingleton<IFilterService, LinkFilterService>();
            services.AddSingleton<IFilterService, ProfanityFilterService>();
            services.AddSingleton<IList<IFilterService>>(s => s.GetServices<IFilterService>().ToList());
            services.AddSingleton<IFilterFactory, FilterFactory>();
            services.AddSingleton<IPunishmentFactory, PunishmentFactory>();
            services.AddSingleton<IWarningService, WarningService>();
            services.AddSingleton<IUserWarningsService, UserWarningsService>();
            services.AddSingleton<IBypassCheck, BypassCheck>();
            services.AddSingleton<IModerationModuleUtils, ModerationModuleUtils>();
            // Moderation Module punishments
            services.AddSingleton<IPunishment, BanPunishment>();
            services.AddSingleton<IPunishment, KickPunishment>();
            services.AddSingleton<IPunishment, AddRolePunishment>();
            services.AddSingleton<IList<IPunishment>>(s => s.GetServices<IPunishment>().ToList());
            services.AddSingleton<IPunishmentService, PunishmentService>();

            // Commands
            services.AddSingleton<IModule, CommandManager>();
            services.AddSingleton<ICommand, HelpCommand>();
            services.AddSingleton<ICommand, AboutCommand>();

            // All Modules
            services.AddSingleton<IList<IModule>>(s => s.GetServices<IModule>().ToList());
            services.AddSingleton<IList<ICommand>>(s => s.GetServices<ICommand>().ToList());

            // HTTP Clients
            services.AddHttpClient<IDiscordAPIClient, DiscordAPIClient>();

            // Discord Services
            services.AddScoped<IDiscordUserGuildService, DiscordUserGuildService>();
            services.AddScoped<IDiscordUserService, DiscordUserService>();
            services.AddScoped<IDiscordGuildService, DiscordGuildService>();
            services.AddSingleton<IMessageService, MessageService>();

            // Database Services
            services.AddScoped(typeof(IEntityService<>), typeof(EntityService<>));
            services.AddSingleton(typeof(IModuleSettingsService<>), typeof(ModuleSettingsService<>));
            services.AddSingleton(typeof(EntityChangedDispatcher<>));

            // Cache
            services.AddSingleton<ICache>(new FluentIMemoryCache(new MemoryCache(new MemoryCacheOptions())));

            // Background Job Service and Jobs
            services.AddSingleton<IJobService, JobService>();
            services.AddTransient<JobActivator, ServiceProviderJobActivator>();
            services.AddTransient<RecurringReminderMessageJob>();
            services.AddSingleton<RemoveExpiredWarningsJob>();
            services.AddSingleton<RemovePunishmentJob>();

            // MVC
            services.AddMvc(options =>
            {
                options.Filters.Add(new ModelStateValidationFilter());
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })       
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Ignore self-ref loop to prevent error when serializing entities with navigation properties when using hangfire.
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Entity Framework
            services.AddDbContext<VolvoxHeliosContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("VolvoxHeliosDatabase")));

            services.AddHangfire(gc =>
            {
                gc.UseSqlServerStorage(Configuration.GetConnectionString("VolvoxHeliosDatabase"), new SqlServerStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true
                });
            });

            //TODO: Refactor to make cleaner
            services.AddHealthChecks()
                .AddCheck("sql", () =>
                {
                    using (var connection = new SqlConnection(Configuration.GetConnectionString("VolvoxHeliosDatabase")))
                    {
                        try
                        {
                            connection.Open();
                        }
                        catch (SqlException)
                        {
                            return HealthCheckResult.Unhealthy();
                        }
                        return HealthCheckResult.Healthy();
                    }
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, VolvoxHeliosContext context)
        {
            if (_env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseHangfireDashboard();
            }
            else
            {
                app.UseExceptionHandler("/Error/Error");
                app.UseStatusCodePagesWithReExecute("/Error/Errors/{0}");
                app.UseHsts();

                loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
            }

            // Update the database.
            context.Database.Migrate();

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

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Activator = app.ApplicationServices.GetRequiredService<JobActivator>()
            });

            app.UseHealthChecks("/health");
        }
    }
}
