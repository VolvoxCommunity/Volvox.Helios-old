using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter
{
    [Table("mod_banned_words")]
    public class BannedWord
    {
        [Key] public int Id { get; set; }

        [Required] public virtual ulong GuildId { get; set; }

        [Required] [ForeignKey("GuildId")] public virtual ProfanityFilter ProfanityFilter { get; set; }

        [Required] [MaxLength(26)] public string Word { get; set; }
    }
}