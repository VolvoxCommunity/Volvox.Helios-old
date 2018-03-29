using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Web.HostedServices.Bot;

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
            // Authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })

                // Cookie Authentication
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/signout";
                })

                // Discord Authentication
                .AddDiscord(options =>
                {
                    options.ClientId = Configuration["Discord:ClientID"];
                    options.ClientSecret = Configuration["Discord:ClientSecret"];
                    options.Scope.Add("identify");
                });
            
            // Hosted Services
            services.AddSingleton<IHostedService, BotHostedService>();

            // Settings
            services.AddSingleton<IDiscordSettings, DiscordSettings>();
            
            // Bot
            services.AddSingleton<IBot, Bot>();

            // Modules

            // All Modules
            services.AddTransient<IList<IModule>>(module => module.GetServices<IModule>().ToList());

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

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