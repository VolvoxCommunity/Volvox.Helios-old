using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volvox.Helios.Service.Discord.UserGuild
{
    public interface IDiscordUserGuildService
    {
        /// <summary>
        ///     Get all of the logged in users guilds.
        /// </summary>
        /// <returns>List of all of the logged in users guilds.</returns>
        Task<List<Domain.Discord.UserGuild>> GetUserGuilds();
    }
}