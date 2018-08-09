using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Core.Utilities
{
    public class DiscordSettings : IDiscordSettings
    {
        public DiscordSettings(IConfiguration config)
        {
            Token = config["Discord:Token"];
        }

        public string Token { get; set; }
    }
}