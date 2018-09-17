using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.PollModule;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/polls/{guildId}")]
    [IsUserGuildAdminFilter]
    public class PollController : Controller
    {
        #region Poll
        [HttpGet("NewPoll")]
        public async Task<IActionResult> NewPoll(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            // All channels for guild.
            var channels = await guildService.GetChannels(guildId);

            // Text channels for guild.
            var textChannels = channels.Where(x => x.Type == 0).ToList();

            var viewModel = new NewPollViewModel
            {
                Channels = new SelectList(textChannels, "Id", "Name"),
                TotOptions = 10,
                GuildId = guildId.ToString()
            };

            return View(viewModel);
        }

        [HttpPost("NewPoll")]
        public async Task<IActionResult> NewPoll(ulong guildId, NewPollViewModel viewModel, List<string> options)
        {

            var validOptions = options.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            
            if (validOptions.Count < 2)
            {
                ModelState.AddModelError("All", "Please provide atleast 2 valid options.");
                return View(viewModel);
            }

            if(!ModelState.IsValid) return View(viewModel);

            return RedirectToAction("Index");
        }
        #endregion
    }
}
