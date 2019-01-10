using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class DadModuleSettingViewModel
    {
        public ulong GuildId { get; set; }

        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(false)]
        public bool Enabled { get; set; }

        [DisplayName("Message Response Cooldown")]
        public int ResponseCooldownMinutes { get; set; }
    }
}
