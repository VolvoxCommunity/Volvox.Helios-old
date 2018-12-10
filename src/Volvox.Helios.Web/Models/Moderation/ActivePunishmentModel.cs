using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Web.Models.Moderation
{
    public class ActivePunishmentModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // ID of punishment in punishments table. This is used to check if a user already has a certain punishment applied.
        public int PunishmentId { get; set; }

        public ulong? RoleId { get; set; }

        public PunishType PunishType { get; set; }

        public DateTimeOffset PunishmentExpires { get; set; }

        public bool RemovePunishment { get; set; }
    }
}
