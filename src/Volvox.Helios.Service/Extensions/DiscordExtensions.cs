using System;
using System.Collections.Generic;
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
    }
}
