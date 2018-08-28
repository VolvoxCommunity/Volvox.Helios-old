using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class StreamerRoleSettingsViewModel
    {
        public SelectList Roles { get; set; }

        [Required]
        [DisplayName("Role")]
        public ulong RoleId { get; set; }

        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }
    }
}