using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public class WhitelistedChannel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ulong ChannelId { get; set; }

        [Required]
        public WhitelistType WhitelistType { get; set; }
    }
}
