using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Web.Models.Moderation
{
    public class UserModel
    {
        public ulong Id { get; set; }

        public string Username { get; set; }

        public string AvatarUrl { get; set; }
    }
}
