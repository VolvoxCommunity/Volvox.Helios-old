using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class StreamerChannelSettingsViewModel
    {
        [Required]
        [DisplayName("Enable channel")]
        [DefaultValue(false)]
        public bool Enabled { get; set; }

        [Required]
        [DisplayName("Remove messages on conclusion")]
        [DefaultValue(false)]
        public bool RemoveMessages { get; set; }
    }
}