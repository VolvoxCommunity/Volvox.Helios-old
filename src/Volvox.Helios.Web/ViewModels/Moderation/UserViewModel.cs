using System.Collections.Generic;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Web.ViewModels.Moderation
{
    public class UserViewModel
    {
        public ulong UserId { get; set; }

        public List<ActivePunishment> ActivePunishments { get; set; } = new List<ActivePunishment>();

        public List<Warning> Warnings { get; set; } = new List<Warning>();
    }
}
