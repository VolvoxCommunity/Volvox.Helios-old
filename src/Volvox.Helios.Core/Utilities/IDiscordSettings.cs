using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Core.Utilities
{
    public interface IDiscordSettings
    {
        string Token { get; set; }
        IConfiguration Config { get; }
    }
}
