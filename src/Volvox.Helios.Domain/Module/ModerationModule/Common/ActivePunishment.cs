﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    [Table("mod_ActivePunishments")]
    public class ActivePunishment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserWarnings User { get; set; }

        // ID of punishment in punishments table. This is used to check if a user already has a certain punishment applied.
        [Required]
        public int PunishmentId { get; set; }

        public ulong? RoleId { get; set; }

        [Required]
        public PunishType PunishType { get; set; }

        [Required]
        public DateTimeOffset PunishmentExpires { get; set; }
    }
}
