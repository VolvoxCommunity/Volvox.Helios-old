using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public class WhitelistedRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoleName { get; set; }

        [Required]
        public WhitelistType WhitelistType { get; set; }
    }
}
