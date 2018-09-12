using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Domain.Module.ChatTracker
{
    public class Message
    {
        [Key]
        public ulong Id { get; set; }

        public ulong AuthorId { get; set; }

        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }
    }
}