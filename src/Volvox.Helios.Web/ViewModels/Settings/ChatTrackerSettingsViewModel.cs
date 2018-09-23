using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class ChatTrackerSettingsViewModel
    {
        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }
    }
}