using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Domain.Module
{
    public class CleanChatChannel
    {
        [Key]
        public ulong Id { get; set; }

        public ulong GuildId { get; set; }
    }
}