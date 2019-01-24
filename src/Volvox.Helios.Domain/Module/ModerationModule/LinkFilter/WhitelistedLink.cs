using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volvox.Helios.Domain.Module.ModerationModule.LinkFilter
{
    [Table("mod_whitelisted_links")]
    public class WhitelistedLink
    {
        [Key] public int Id { get; set; }

        [Required] public virtual ulong GuildId { get; set; }

        [ForeignKey("GuildId")] public virtual LinkFilter LinkFilter { get; set; }

        [Required] public string Link { get; set; }
    }
}