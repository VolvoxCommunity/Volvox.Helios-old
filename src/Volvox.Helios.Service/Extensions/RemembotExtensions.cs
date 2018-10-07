using System;
using System.Collections.Generic;
using System.Text;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Service.Extensions
{
    public static class RemembotExtensions
    {
        /// <summary>
        ///     Gets a short status message based on the <see cref="RecurringReminderMessage.FaultType"/> of the <see cref="RecurringReminderMessage"/>.
        /// </summary>
        /// <param name="reminderMessage"></param>
        /// <returns></returns>
        public static string GetFaultShortMessage(this RecurringReminderMessage reminderMessage)
        {
            switch (reminderMessage.Fault)
            {
                case RecurringReminderMessage.FaultType.InvalidChannel:
                    return "Invalid Channel";

                case RecurringReminderMessage.FaultType.InvalidCron:
                    return "Invalid Cron Expression";

                default:
                    return null;
            }
        }

        /// <summary>
        ///     Gets a long status message based on the <see cref="RecurringReminderMessage.FaultType"/> of the <see cref="RecurringReminderMessage"/>.
        /// </summary>
        /// <param name="reminderMessage"></param>
        /// <returns></returns>
        public static string GetFaultLongMessage(this RecurringReminderMessage reminderMessage)
        {
            switch (reminderMessage.Fault)
            {
                case RecurringReminderMessage.FaultType.InvalidChannel:
                    return "The channel for this reminder no longer exists. Please update to an existing channel.";

                case RecurringReminderMessage.FaultType.InvalidCron:
                    return "The Cron expression that specifies the recurrence for this reminder is not valid.";

                default:
                    return null;
            }
        }
    }
}
