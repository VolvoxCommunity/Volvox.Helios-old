using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule
{
    [Table("mod_users")]
    public class UserWarnings
    {
        [Key] public int Id { get; set; }

        [Required] public virtual ulong GuildId { get; set; }

        [ForeignKey("GuildId")] public ModerationSettings ModerationSettings { get; set; }

        [Required] public ulong UserId { get; set; }

        [Required] public virtual List<Warning> Warnings { get; set; } = new List<Warning>();

        [Required] public virtual List<ActivePunishment> ActivePunishments { get; set; } = new List<ActivePunishment>();
    }
}