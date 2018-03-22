using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Web.Utilities
{
    public class DiscordSettings : IDiscordSettings
    {
        public DiscordSettings(IConfiguration config)
        {
            Token = config["Discord:Token"];
        }

        public string Token { get; set; }
    }
}
