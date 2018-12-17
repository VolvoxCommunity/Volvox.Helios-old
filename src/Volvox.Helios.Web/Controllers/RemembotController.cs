using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.Extensions;
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
        public async Task<IActionResult> EditRecurringReminder(ulong guildId, [FromQuery]long rid)
        {
            var channels = await _guildService.GetChannels(guildId);
            var textChannels = channels.Where(x => x.Type == 0);

            if (rid == default(long))
            {
                var newVm = new EditRecurringReminderMessageViewModel
                {
                    Channels = new SelectList(textChannels, "Id", "Name"),
                    Enabled = true,
                    GuildId = guildId,
                    CronExpression = Cron.Minutely()
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
                CronExpression = reminder.CronExpression,
                Enabled = reminder.Enabled,
                Message = reminder.Message,
                FaultMessage = reminder.GetFaultLongMessage(),
                ChannelId = reminder.ChannelId,
                ChannelName = selectedChannel.Name,
                FaultType = (int)reminder.Fault,
                Id = reminder.Id
            };

            return View(editVm);
        }

        [HttpPost]
        public async Task<IActionResult> EditRecurringReminder(ulong guildId, EditRecurringReminderMessageViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (vm.Id == default(long))
            {
                var newReminder = new RecurringReminderMessage
                {
                    ChannelId = vm.ChannelId,
                    Enabled = vm.Enabled,
                    Message = vm.Message.Replace("<br>", "\n"),
                    CronExpression = vm.CronExpression,
                    GuildId = guildId
                };

                await _reminderService.Create(newReminder);
                return RedirectToAction("RemembotSettings", "Settings");
            }

            var reminder = await _reminderService.Find(vm.Id);
            reminder.ChannelId = vm.ChannelId;
            reminder.Enabled = vm.Enabled;
            reminder.Message = vm.Message.Replace("<br>", "\n");
            reminder.CronExpression = vm.CronExpression;

            await _reminderService.Update(reminder);

            return RedirectToAction("RemembotSettings", "Settings");
        }

        [HttpPost("DeleteRecurringReminder")]
        public async Task<IActionResult> DeleteRecurringReminder(ulong guildId, long id)
        {
            var reminder = await _reminderService.Find(id);
            if (reminder is null)
                return RedirectToAction("RemembotSettings", "Settings");

            await _reminderService.Remove(reminder);
            return RedirectToAction("RemembotSettings", "Settings");
        }
    }
}