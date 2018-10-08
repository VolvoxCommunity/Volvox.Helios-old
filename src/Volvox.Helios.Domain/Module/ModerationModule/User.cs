using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ulong UserId { get; set; }

        [Required]
        public ModerationSettings ModerationSettings { get; set; }

        [Required]
        public List<Warning> Warnings { get; set; }
    }
}
