using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.Poll;
using Volvox.Helios.Web.ViewModels.PollModule;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/polls/{guildId}")]
    [IsUserGuildAdminFilter]
    public class PollController : Controller
    {
        private readonly IEntityService<Poll> _entityServicePoll;
        private readonly IEntityService<PollSettings> _entityServicePollSettings;

        public PollController(IEntityService<Poll> polls, IEntityService<PollSettings> pollSettings)
        {
            _entityServicePoll = polls;
            _entityServicePollSettings = pollSettings;
        }

        #region NewPoll
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
        public async Task<IActionResult> NewPoll([FromServices] IDiscordGuildService guildService, [FromServices] IMessageService messageService,
            ulong guildId, NewPollViewModel viewModel, List<string> options)
        {
            // All channels for guild.
            var channels = await guildService.GetChannels(guildId);

            // Text channels for guild.
            var textChannels = channels.Where(x => x.Type == 0).ToList();

            // Authentication - make sure channel belongs to the guild (prevents exploitation by providing a channel that isn't theirs, if they're authorized).
            // Note - this should be extracted into a seperate web filter.
            if (textChannels.FirstOrDefault(x => x.Id == viewModel.ChannelId) == null)
            {
                ModelState.AddModelError("All", "You are not authorized to perform this action.");

                viewModel.Channels = new SelectList(textChannels, "Id", "Name");

                return View(viewModel);
            }

            var validOptions = options.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            // Make sure user supplied valid no of options
            if (validOptions.Count < 2 || validOptions.Count > 10)
            {
                ModelState.AddModelError("All", "Please provide between 2 and 10 valid options.");

                viewModel.Channels = new SelectList(textChannels, "Id", "Name");

                return View(viewModel);
            }

            if(!ModelState.IsValid) return View(viewModel);

            var poll = new StringBuilder();

            // Add poll title.
            poll.Append(":small_blue_diamond: " + viewModel.Title + Environment.NewLine);

            // Unicode representations of discord's 0-10 emoticons.
            var discordNumbers = new string[11] { "0\u20e3", "1\u20e3", "2\u20e3", "3\u20e3", "4\u20e3", "5\u20e3", "6\u20e3", "7\u20e3", "8\u20e3", "9\u20e3", "🔟" };

            // Add options to poll.
            for (var i = 0; i < validOptions.Count; i++)
                poll.Append($"{discordNumbers[i + 1]} {validOptions[i]}{Environment.NewLine}");

            // Post message and get message details.
            var message = await messageService.Post(viewModel.ChannelId, poll.ToString());

            // Save poll to database
            await _entityServicePoll.Create(new Poll()
            {
                ChannelId = viewModel.ChannelId,
                MessageId = message.Id,
                PollTitle = viewModel.Title,
                GuildId = guildId
            });

            // Add reactions to message to act as voting buttons.
            for (var i = 1; i < validOptions.Count + 1; i++)
                await messageService.AddReaction(message, new Emoji(discordNumbers[i]));

            // Create poll settings entry for guild in db, if doesn't exist.
            if (await _entityServicePollSettings.Find(guildId) == null)
            {
                await _entityServicePollSettings.Create(new PollSettings()
                {
                    GuildId = guildId
                });
            }     

            return RedirectToAction("Index");
        }
        #endregion

        #region ViewPoll
        [HttpGet("ViewPoll")]
        public async Task<IActionResult> ViewPoll(ulong guildId, [FromServices] IEntityService<Poll> entityServicePolls)
        {
            var polls = await entityServicePolls.Get(p => p.GuildId == guildId);

            // Format polls for use in selectlist
            var formattedPolls = polls.Select(p => new PollSelectListViewModel() {
                ChannelId = p.ChannelId,
                MessageId = p.MessageId,
                Title = p.PollTitle
            }).ToList();

            var viewModel = new ViewPollsViewModel()
            {
                // ConcattedIds is a string in format messageId-channelId, so can store both in select list.
                Polls = new SelectList(formattedPolls, "ConcattedIds", "Title"),
                Title = "",
                Options = new List<OptionModel>()
            };

            return View(viewModel);
        }

        #endregion
    }
}
