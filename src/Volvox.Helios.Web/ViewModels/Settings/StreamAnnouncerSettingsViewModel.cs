using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class StreamAnnouncerSettingsViewModel
    {
        public SelectList Channels { get; set; }
        
        [Required]
        [DisplayName("Channel")]
        public ulong ChannelId { get; set; }

        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        [Required]
        [DisplayName("Delete messages on stream conclusion")]
        [DefaultValue(false)]
        public bool RemoveMessages { get; set; }
    }
}