using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class DadModuleSettings : ModuleSettings
    {
        /// <summary>
        /// The last date and time a dad message was sent.
        /// </summary>
        public DateTime? LastDadResponseUtc { get; set; }

        /// <summary>
        /// Cooldown time in minutes for Dad's "Hi _, "I'm dad" message. Defaults to 15;
        /// </summary>
        public int DadResponseCooldownMinutes { get; set; } = 15;
    }
}
