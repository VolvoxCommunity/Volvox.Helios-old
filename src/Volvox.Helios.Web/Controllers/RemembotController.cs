using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/Remembot/{guildId}")]
    [IsUserGuildAdminFilter]
    public class RemembotController : Controller
    {
        private readonly IEntityService<RecurringReminderMessage> _reminderService;
        private readonly IDiscordGuildService _guildService;

        public RemembotController(IEntityService<RecurringReminderMessage> reminderService,
            IDiscordGuildService guildService)
        {
            _reminderService = reminderService;
            _guildService = guildService;
        }

        [HttpGet]
        public async Task<IActionResult> EditRecurringReminder(ulong guildId, [FromQuery]Guid rid)
        {
            var channels = await _guildService.GetChannels(guildId);
            var textChannels = channels.Where(x => x.Type == 0);
            if(rid == default(Guid))
            {
                var newVm = new EditRecurringReminderMessageViewModel
                {
                    Channels = new SelectList(textChannels, "Id", "Name"),
                    Enabled = true,
                    GuildId = guildId
                };
                return View(newVm);
            }

            var reminder = await _reminderService.Find(rid);
            if(reminder is null)
                return RedirectToAction("RemembotSettings", "Settings");

            var selectedChannel = channels.Find(x => x.Id == reminder.ChannelId);
            var editVm = new EditRecurringReminderMessageViewModel
            {
                GuildId = guildId,
                Channels = new SelectList(textChannels, "Id", "Name", selectedChannel),
                DayOfMonth = reminder.DayOfMonthExpression,
                DayOfWeek = reminder.DayOfWeekExpression,
                Enabled = reminder.Enabled,
                Hours = reminder.HoursExpression,
                Message = reminder.Message,
                Minutes = reminder.MinutesExpression,
                Month = reminder.MonthExpression,
                Id = reminder.Id
            };

            return View(editVm);
        }

        [HttpPost]
        public async Task<IActionResult> EditRecurringReminder(ulong guildId, EditRecurringReminderMessageViewModel vm)
        {
            // TODO - Run validations

            if (vm.Id == default(Guid))
            {
                var newReminder = new RecurringReminderMessage
                {
                    ChannelId = vm.ChannelId,
                    DayOfMonthExpression = vm.DayOfMonth,
                    DayOfWeekExpression = vm.DayOfWeek,
                    Enabled = vm.Enabled,
                    HoursExpression = vm.Hours,
                    Message = vm.Message,
                    MinutesExpression = vm.Minutes,
                    MonthExpression = vm.Month,
                    GuildId = guildId
                };

                await _reminderService.Create(newReminder);
                return RedirectToAction("RemembotSettings", "Settings");
            }

            var reminder = await _reminderService.Find(vm.Id);
            reminder.ChannelId = vm.ChannelId;
            reminder.DayOfMonthExpression = vm.DayOfMonth;
            reminder.DayOfWeekExpression = vm.DayOfWeek;
            reminder.Enabled = vm.Enabled;
            reminder.HoursExpression = vm.Hours;
            reminder.Message = vm.Message;
            reminder.MinutesExpression = vm.Minutes;
            reminder.MonthExpression = vm.Month;

            await _reminderService.Update(reminder);

            return RedirectToAction("RemembotSettings", "Settings");
        }

        [HttpPost("DeleteRecurringReminder")]
        public async Task<IActionResult> DeleteRecurringReminder(ulong guildId, Guid id)
        {
            var reminder = await _reminderService.Find(id);
            if (reminder is null)
                return RedirectToAction("RemembotSettings", "Settings");

            await _reminderService.Remove(reminder);
            return RedirectToAction("RemembotSettings", "Settings");
        }
    }
}