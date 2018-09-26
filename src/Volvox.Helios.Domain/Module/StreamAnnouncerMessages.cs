using System.ComponentModel.DataAnnotations;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.StreamAnnouncer
{
    public class StreamAnnouncerMessages
    {
        [Key]
        public int Id { get; set; }

        public ulong UserId { get; set; }

        public ulong MessageId { get; set; }

        public ulong ChannelId { get; set; }

        public virtual StreamAnnouncerSettings StreamAnnouncerSettings { get; set; }
    }
}
