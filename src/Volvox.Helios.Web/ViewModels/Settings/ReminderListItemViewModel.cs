using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class ReminderListItemViewModel
    {
        public long Id { get; set; }
        public string ChannelName { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public bool IsFaulted { get; set; }
    }
}
