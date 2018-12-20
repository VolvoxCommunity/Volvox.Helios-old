using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Volvox.Helios.Service.Discord.User
{
    public interface IDiscordUserService
    {
        /// <summary>
        ///     Get the specified user.
        /// </summary>
        /// <param name="userId">Id of the user to get.</param>
        /// <returns>Specified user.</returns>
        Task<Domain.Discord.DiscordUser> GetUser(ulong userId);

        /// <summary>
        ///     Get the users from specified guild.
        /// </summary>
        /// <param name="guildId">Id of guild from which to get users.</param>
        /// <returns>List of users.</returns>
        List<SocketGuildUser> GetUsers(ulong guildId);
    }
}