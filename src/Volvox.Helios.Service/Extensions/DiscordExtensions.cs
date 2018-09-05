using System.Collections.Generic;
using System.Linq;
using Volvox.Helios.Domain.Discord;

namespace Volvox.Helios.Service.Extensions
{
    public static class DiscordExtensions
    {
        /// <summary>
        ///     Filter a list of guilds to only the ones that the user is an administrator of.
        /// </summary>
        /// <param name="userGuilds">List of guilds.</param>
        /// <returns>List of guilds that the user is an administrator of.</returns>
        public static List<UserGuild> FilterAdministrator(this IEnumerable<UserGuild> userGuilds)
        {
            var filteredGuilds = new List<UserGuild>();

            foreach (UserGuild userGuild in userGuilds)
            {
                const int admin = 0x00000008;

                if (( userGuild.Permissions & admin ) == admin) filteredGuilds.Add(userGuild);
            }

            return filteredGuilds;
        }

        /// <summary>
        ///     Filter a list of channels by type.
        /// </summary>
        /// <param name="channels">List of channels.</param>
        /// <param name="type">Type of channel.</param>
        /// <returns>List of channels with the specified type.</returns>
        public static List<Channel> FilterChannelType(this IEnumerable<Channel> channels, int type)
        {
            return channels.Where(c => c.Type == type).ToList();
        }

        /// <summary>
        ///     Filter a list of guilds to a list of ids.
        /// </summary>
        /// <param name="guilds">List of guilds.</param>
        /// <param name="ids">List of ids.</param>
        /// <returns>List of guilds with they specified id.</returns>
        public static List<Guild> FilterGuildsByIds(this IEnumerable<Guild> guilds, List<ulong> ids)
        {
            return guilds.Where(g => ids.Any(i => i == g.Id)).ToList();
        }

        /// <summary>
        ///     Remove the managed roles from the list.
        /// </summary>
        /// <param name="roles">List of roles.</param>
        /// <returns>List of unmanaged roles.</returns>
        public static IEnumerable<Role> RemoveManaged(this IEnumerable<Role> roles)
        {
            return roles.Where(r => !r.Managed);
        }
    }
}