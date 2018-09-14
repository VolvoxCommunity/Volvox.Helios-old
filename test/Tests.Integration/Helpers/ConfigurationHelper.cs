using Microsoft.Extensions.Configuration;

namespace Tests.Integration.Helpers
{
    public class ConfigurationHelper
    {
        public static IConfiguration Configuration;

        static ConfigurationHelper()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Volvox.Helios.Web.Startup>()
                .AddEnvironmentVariables();
            Configuration = configBuilder.Build();
        }
    }
}