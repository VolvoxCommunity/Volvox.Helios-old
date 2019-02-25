
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class CleanChatSettingsViewModel
    {
        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        public ulong GuildId { get; set; }
        
        public List<ulong> Channels { get; set; }

        public int MessageDuration { get; set; }
    }
}
