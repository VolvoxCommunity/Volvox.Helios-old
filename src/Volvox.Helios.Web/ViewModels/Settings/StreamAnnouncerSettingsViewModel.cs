using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Web.Models;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class StreamAnnouncerSettingsViewModel
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

        public StreamAnnouncerChannelSettingsViewModel ChannelSettings { get; set; }
    }
}