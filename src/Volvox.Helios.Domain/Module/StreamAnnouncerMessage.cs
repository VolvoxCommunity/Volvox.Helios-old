using System.ComponentModel.DataAnnotations;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.StreamAnnouncer
{
    public class StreamAnnouncerMessage
    {
        [Key]
        public int Id { get; set; }

        public ulong UserId { get; set; }

        public ulong MessageId { get; set; }

        public ulong ChannelId { get; set; }

        public ulong GuildId { get; set; }

        public virtual StreamerSettings StreamerSettings { get; set; }
    }
}
