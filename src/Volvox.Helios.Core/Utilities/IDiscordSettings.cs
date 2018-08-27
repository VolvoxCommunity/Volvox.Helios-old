using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Core.Utilities
{
    public interface IDiscordSettings
    {
        string Token { get; }

        string ClientId { get; }

        IConfiguration Config { get; }
    }
}