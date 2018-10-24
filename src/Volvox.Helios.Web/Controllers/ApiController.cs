using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.UserGuild;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.Poll;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/api")]
    public class ApiController : Controller
    {
        // GET
        [HttpGet("GetGuildChannels")]
        public async Task<object> GetGuildChannels([FromServices] IDiscordGuildService guildService, ulong guildId)
        {
            var channels = await guildService.GetChannels(guildId);

            // Format the ulong to string.
            return channels.FilterChannelType(0).Select(c => new
            {
                id = c.Id.ToString(),
                name = c.Name
            });
        }

        [IsUserGuildAdminFilter]
        [HttpGet("GetChannelSettingsAnnouncer")]
        public async Task<StreamerChannelSettingsViewModel> GetChannelSettingsAnnouncer(
            [FromServices] IDiscordUserGuildService userGuildService,
            [FromServices] IModuleSettingsService<StreamerSettings> streamerSettingsService, ulong guildId,
            ulong channelId)
        {
            // All channel's settings in guild.
            var allChannelSettings = await streamerSettingsService.GetSettingsByGuild(guildId, x => x.ChannelSettings);

            if (allChannelSettings == null)
                return new StreamerChannelSettingsViewModel
                {
                    Enabled = false,
                    RemoveMessages = false
                };

            // Settings for specific channel.
            var channelSettings = allChannelSettings.ChannelSettings.FirstOrDefault(x => x.ChannelId == channelId);

            var isEnabled = channelSettings != null;

            var settings = new StreamerChannelSettingsViewModel
            {
                Enabled = isEnabled,
                RemoveMessages = isEnabled && channelSettings.RemoveMessage
            };

            return settings;
        }

        [HttpGet("GetUserAdminGuilds")]
        public async Task<object> GetUserAdminGuilds([FromServices] IDiscordUserGuildService userGuildService,
            [FromServices] IBot bot, bool inGuild = false)
        {
            var guilds = await userGuildService.GetUserGuilds();

            if (inGuild) guilds.RemoveAll(g => !bot.IsBotInGuild(g.Guild.Id));

            // Format the ulong to string.
            return guilds.FilterAdministrator().Select(g => new
            {
                id = g.Guild.Id.ToString(),
                name = g.Guild.Name,
                icon = g.Guild.Icon
            });
        }

        [HttpGet("IsBotInGuild")]
        public bool IsBotInGuild(ulong guildId, [FromServices] IBot bot)
        {
            return bot.IsBotInGuild(guildId);
        }

        [HttpGet("GetPollTitles")]
        public async Task<List<string>> GetPollTitles(ulong guildId,
            [FromServices] IEntityService<Poll> entityServicePolls)
        {
            var polls = await entityServicePolls.Get(x => x.GuildId == guildId);

            return polls.Select(g => g.PollTitle).ToList();
        }

        [HttpGet("GetPollData")]
        public async Task<object> GetPollData(ulong channelId, ulong messageId,
            [FromServices] IMessageService messageService)
        {
            var poll = await messageService.GetMessage(channelId, messageId);

            var formattedPoll = new PollModel();

            if (poll == null) return formattedPoll;

            // Splitting by newline will provide array with title at index 0, and the rest will be the options
            var pollDetails = poll.Content.Split(Environment.NewLine);

            formattedPoll.Title = pollDetails[0];

            var pollOptions = new List<OptionModel>();

            var discordNumbers = MessageService.DiscordNumberEmotes;

            for (var i = 1; i < pollDetails.GetLength(0); i++)
                pollOptions.Add(new OptionModel
                {
                    Option = pollDetails[i],
                    VoteCount = poll.Reactions[new Emoji(discordNumbers[i])].ReactionCount
                });

            formattedPoll.Options = pollOptions;

            return formattedPoll;
        }
    }
}