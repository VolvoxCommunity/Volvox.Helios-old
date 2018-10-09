using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public class Channel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ulong ChannelId { get; set; }
    }
}
