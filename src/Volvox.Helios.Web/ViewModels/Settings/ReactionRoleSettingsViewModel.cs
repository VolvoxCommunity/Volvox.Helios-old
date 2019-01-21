using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Web.ViewModels.ReactionRoles;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class ReactionRoleSettingsViewModel
    {
        public ulong GuildId { get; set; }

        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        public List<ReactionRolesMessageViewModel> ReactionRolesMessages { get; set; }
    }
}
