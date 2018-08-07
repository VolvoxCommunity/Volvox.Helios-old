using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Core.Utilities
{
    public class DiscordSettings : IDiscordSettings
    {
        public DiscordSettings(IConfiguration config)
        {
            Token = config["Discord:Token"];
            Config = config;
        }

        public string Token { get; set; }
        public IConfiguration Config { get; }
    }
}
