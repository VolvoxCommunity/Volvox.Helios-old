using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Volvox.Helios.Domain.Discord;

namespace Volvox.Helios.Service.Extensions
{
    public static class DiscordExtensions
    {
        /// <summary>
        /// Filter a list of Guilds to only the ones that a user is an adminstrator of.
        /// </summary>
        /// <param name="guilds">List of Guilds.</param>
        /// <returns>List of Guilds that the user is an adminstrator of.</returns>
        public static List<Guild> FilterAdministrator(this List<Guild> guilds)
        {
            var filteredGuilds = new List<Guild>(); 

            foreach (var guild in guilds)
            {
                const int admin = 0x00000008;

                if ((guild.Permissions & admin) == admin)
                {
                    filteredGuilds.Add(guild);
                }
            }

            return filteredGuilds;
        }

        /// <summary>
        /// Filter a list of channels by type.
        /// </summary>
        /// <param name="channels">List of channels.</param>
        /// <param name="type">Type of channel.</param>
        /// <returns>List of channels with the specified type.</returns>
        public static List<Channel> FilterChannelType(this List<Channel> channels, int type)
        {
            return channels.Where(c => c.Type == type).ToList();
        }

        /// <summary>
        /// Filter a list of guilds to a list of ids.
        /// </summary>
        /// <param name="guilds">List of guilds.</param>
        /// <param name="ids">List of ids.</param>
        /// <returns>List of guilds with they specified id.</returns>
        public static List<Guild> FilterGuildsByIds(this List<Guild> guilds, List<ulong> ids)
        {
            return guilds.Where(g => ids.Any(i => i == g.Id)).ToList();
        }
    }
}
