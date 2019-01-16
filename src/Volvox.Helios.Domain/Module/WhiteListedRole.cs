using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Domain.Module
{
    public class WhiteListedRole
    {
        [Key]
        public ulong RoleId { get; set; }

        public ulong GuildId { get; set; }
    }
}