using System.Threading.Tasks;

namespace Volvox.Helios.Service.Clients
{
    public interface IDiscordAPIClient
    {
        /// <summary>
        ///     Get a specific user.
        /// </summary>
        /// <param name="userId">Id of the user to get.</param>
        /// <returns>JSON of the User object.</returns>
        Task<string> GetUser(ulong userId);

        /// <summary>
        ///     Get all of the currently logged in users guilds.
        /// </summary>
        /// <returns>JSON array of the logged in users guilds.</returns>
        Task<string> GetUserGuilds();

        /// <summary>
        ///     Get all of the channels in the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>JSON array of channels in the guild.</returns>
        Task<string> GetGuildChannels(ulong guildId);

        /// <summary>
        ///     Get all of the roles in the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>JSON array of roles in the guild.</returns>
        Task<string> GetGuildRoles(ulong guildId);
    }
}