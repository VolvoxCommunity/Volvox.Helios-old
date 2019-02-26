
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class CleanChatSettingsViewModel
    {
        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        public ulong GuildId { get; set; }
        
        public SelectList Channels { get; set; }

        public List<ulong> CleanChatChannels { get; set; }

        public int MessageDuration { get; set; }
    }
}
