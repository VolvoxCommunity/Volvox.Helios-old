using System;
using System.Collections.Generic;
using System.Text;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class RemembotSettings : ModuleSettings
    {
        public List<RecurringReminderMessage> RecurringReminders { get; set; }
    }
}
