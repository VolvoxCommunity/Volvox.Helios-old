using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Web
{
    public static class ConfigurationHelper
    {
        public static IConfiguration GetDefaultConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"./appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("./modulemetadata.json")
                .AddUserSecrets<Startup>().Build();
        }
    }
}