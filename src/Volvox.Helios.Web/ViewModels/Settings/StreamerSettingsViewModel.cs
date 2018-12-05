using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class StreamerSettingsViewModel
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

        public StreamerChannelSettingsViewModel ChannelSettings { get; set; }

        [Required]
        [DisplayName("Enabled")]
        public bool StreamerRoleEnabled { get; set; }

        public SelectList Roles { get; set; }

        [Required]
        [DisplayName("Role")]
        public ulong RoleId { get; set; }
    }
}