using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.Models;
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
            return channels.FilterChannelType(0).Select(c => new {id = c.Id.ToString(), name = c.Name});
        }

        [IsUserGuildAdminFilter]
        [HttpGet("GetChannelSettingsAnnouncer")]
        public async Task<StreamAnnouncerChannelSettingsViewModel> GetChannelSettingsAnnouncer([FromServices] IDiscordUserGuildService userGuildService,
            [FromServices] IModuleSettingsService<StreamAnnouncerSettings> streamAnnouncerSettingsService, ulong guildId, ulong channelId)
        {
            // All channel's settings in guild.
            var allChannelSettings = await streamAnnouncerSettingsService.GetSettingsByGuild(guildId, x => x.ChannelSettings);

            if (allChannelSettings == null)
                return new StreamAnnouncerChannelSettingsViewModel() { Enabled = false, RemoveMessages = false };

            // Settings for specific channel.
            var channelSettings = allChannelSettings.ChannelSettings.FirstOrDefault(x => x.ChannelId == channelId);

            var isEnabled = channelSettings != null;

            var settings = new StreamAnnouncerChannelSettingsViewModel()
            {
                Enabled = isEnabled,
                RemoveMessages = isEnabled && channelSettings.RemoveMessage
            };

            return settings;
        }

        [HttpGet("GetUserAdminGuilds")]
        public async Task<object> GetUserAdminGuilds([FromServices] IDiscordUserGuildService userGuildService, [FromServices] IBot bot, bool inGuild = false)
        {
            var guilds = await userGuildService.GetUserGuilds();

            if (inGuild)
            {
                guilds.RemoveAll(g => !bot.IsBotInGuild(g.Guild.Id));
            }

            // Format the ulong to string.
            return guilds.FilterAdministrator().Select(g => new {id = g.Guild.Id.ToString(), name = g.Guild.Name, icon = g.Guild.Icon});
        }

        [HttpGet("IsBotInGuild")]
        public bool IsBotInGuild(ulong guildId, [FromServices] IBot bot)
        {
            return bot.IsBotInGuild(guildId);
        }
    }
}