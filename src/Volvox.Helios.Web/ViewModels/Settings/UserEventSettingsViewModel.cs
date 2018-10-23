using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class UserEventSettingsViewModel
    {
        public SelectList Channels { get; set; }

        public string GuildId { get; set; }

        [Required]
        [DisplayName("Channel")]
        public ulong ChannelId { get; set; }

        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        [Required]
        [DisplayName("User Left")]
        [DefaultValue(true)]
        public bool UserLeftEvent { get; set; }
    }
}