using System.Collections.Generic;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Discord;

namespace Volvox.Helios.Service.Discord.Guild
{
    public interface IDiscordGuildService
    {
        /// <summary>
        ///     Get all of the channels in the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>List of channels in the guild.</returns>
        Task<List<Channel>> GetChannels(ulong guildId);

        /// <summary>
        ///     Get all of the roles in the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>List of roles in the guild.</returns>
        Task<List<Role>> GetRoles(ulong guildId);

        /// <summary>
        ///     Get all of the emojis in the specified guild.
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        Task<List<Emoji>> GetEmojis(ulong guildId);

        /// <summary>
        ///     Get the details of the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>Guild populated with details.</returns>
        Task<Domain.Discord.Guild> GetDetails(ulong guildId);
    }
}