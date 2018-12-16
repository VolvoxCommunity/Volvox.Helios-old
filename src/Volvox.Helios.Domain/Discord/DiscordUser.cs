using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.Discord
{
    public class DiscordUser
    {
        public ulong Id { get; set; }

        public string Username { get; set; }

        public int Discriminator { get; set; }
    }
}
