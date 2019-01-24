using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule
{
    [Table("mod_whitelisted_channels")]
    public class WhitelistedChannel
    {
        [Key] public int Id { get; set; }

        [Required] public virtual ulong GuildId { get; set; }

        [ForeignKey("GuildId")] public virtual ModerationSettings Moderationsettings { get; set; }

        [Required] public ulong ChannelId { get; set; }

        [Required] public FilterType WhitelistType { get; set; }
    }
}