using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.Models.Moderation;
using Volvox.Helios.Domain.Module.ModerationModule;
using System.Collections.Generic;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Web.Controllers
{
    //[Authorize]
    [Route("/moderator/{guildId}")]
    //[IsUserGuildAdminFilter]
    public class ModerationController : Controller
    {
        IModuleSettingsService<ModerationSettings> _moderationSettings;

        IEntityService<ProfanityFilter> _entityServiceProfanityFilter;

        public ModerationController(IModuleSettingsService<ModerationSettings> moderationSettings, IEntityService<ProfanityFilter> entityServiceProfanityFilter)
        {
            _moderationSettings = moderationSettings;

            _entityServiceProfanityFilter = entityServiceProfanityFilter;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ulong guildId)
        {
            var settings = await _moderationSettings.GetSettingsByGuild(guildId, s => s.WhitelistedRoles, s => s.WhitelistedChannels);

            var WhitelistedChannelIds = settings.WhitelistedChannels.Select(c => c.GuildId);

            //var vm = new ModerationGlobal()
            //{
            //    GuildId = guildId,
            //    WhitelistedChannels = settings.WhitelistedChannels,
            //    WhitelistedRoles = settings.WhitelistedRoles
            //};

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ModerationGlobal vm)
        {
            var settings = await _moderationSettings.GetSettingsByGuild(vm.GuildId, s => s.WhitelistedRoles, s => s.WhitelistedChannels);

            var newWhitelistedChannels = new List<WhitelistedChannel>();

            foreach (var channelId in vm.WhitelistedChannelIds)
            {
                // Channel isn't already in database, so add it. Otherwise, skip it.
                if (!settings.WhitelistedChannels.Any(c => c.ChannelId == channelId))
                {
                    // TODO : null checks and check id is valid ulong
                    var channel = new WhitelistedChannel() {
                        ChannelId = channelId,
                        GuildId = vm.GuildId,
                        WhitelistType = WhitelistType.Global
                    };

                    newWhitelistedChannels.Add(channel);
                }
            }

            var newWhitelistedRoles = new List<WhitelistedRole>();

            foreach (var roleId in vm.WhitelistedRoleIds)
            {
                // Role isn't already in database, so add it. Otherwise, skip it.
                if (!settings.WhitelistedChannels.Any(c => c.ChannelId == roleId))
                {
                    // TODO : null checks and check id is valid ulong
                    var role = new WhitelistedRole()
                    { 
                        GuildId = vm.GuildId,
                        RoleId = roleId,
                        WhitelistType = WhitelistType.Global
                    };

                    newWhitelistedRoles.Add(role);
                }
            }
            await _moderationSettings.SaveSettings(settings);

            // Clearing cache prompts the module to refetch moderation settings from the database the next time it needs them, essentially updating them.
            _moderationSettings.ClearCacheByGuild(vm.GuildId);

            return View();
        }
    }
}