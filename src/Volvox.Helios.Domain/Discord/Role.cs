using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.Discord
{
    public class Role
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public int Position { get; set; }

        public bool Managed { get; set; }
    }
}
