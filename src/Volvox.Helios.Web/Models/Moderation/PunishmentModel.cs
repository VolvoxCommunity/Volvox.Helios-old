using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Web.Models.Moderation
{
    public class PunishmentModel
    {
        [NotMapped]
        public int? Index { get; set; }

        public int? PunishmentId { get; set; }

        public short WarningThreshold { get; set; }

        public FilterType WarningType { get; set; }

        public PunishType PunishType { get; set; }

        public int PunishDuration { get; set; }

        public ulong? RoleId { get; set; }

        public SelectList Role { get; set; }

        public bool DeletePunishment { get; set; }
    }
}
