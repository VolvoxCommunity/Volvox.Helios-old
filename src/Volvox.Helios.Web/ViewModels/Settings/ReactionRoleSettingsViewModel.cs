using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class ReactionRoleSettingsViewModel
    {
        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }
    }
}
