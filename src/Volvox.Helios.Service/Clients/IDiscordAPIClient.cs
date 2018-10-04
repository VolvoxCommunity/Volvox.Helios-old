using System.Threading.Tasks;

namespace Volvox.Helios.Service.Clients
{
    public interface IDiscordAPIClient
    {
        Task<string> GetGuildChannels(ulong guildId);
        Task<string> GetGuildRoles(ulong guildId);
        Task<string> GetUserGuilds();
    }
}