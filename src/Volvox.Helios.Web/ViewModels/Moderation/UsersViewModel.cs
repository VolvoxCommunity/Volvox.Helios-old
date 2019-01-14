using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Web.Models.Moderation;

namespace Volvox.Helios.Web.ViewModels.Moderation
{
    public class UsersViewModel
    {
        public List<UserModel> Users { get; set; }

        public int PageNo { get; set; }

        public int TotalPageCount { get; set; }
    }
}
