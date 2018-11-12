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
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;

namespace Volvox.Helios.Web.Controllers
{
    //[Authorize]
    [Route("/moderator/{guildId}")]
    //[IsUserGuildAdminFilter]
    public class ModerationController : Controller
    {
        IModuleSettingsService<ModerationSettings> _moderationSettings;

        IEntityService<ProfanityFilter> _entityServiceProfanityFilter;

        IEntityService<LinkFilter> _entityServiceLinkFilter;

        IEntityService<WhitelistedChannel> _entityServiceWhitelistedChannels;

        IEntityService<WhitelistedRole> _entityServiceWhitelistedRoles;

        public ModerationController(IModuleSettingsService<ModerationSettings> moderationSettings,
            IEntityService<ProfanityFilter> entityServiceProfanityFilter,
            IEntityService<LinkFilter> entityServiceLinkFilter,
            IEntityService<WhitelistedChannel> entityServiceWhitelistedChannels,
            IEntityService<WhitelistedRole> entityServiceWhitelistedRoles)
        {
            _moderationSettings = moderationSettings;

            _entityServiceProfanityFilter = entityServiceProfanityFilter;

            _entityServiceLinkFilter = entityServiceLinkFilter;

            _entityServiceWhitelistedChannels = entityServiceWhitelistedChannels;

            _entityServiceWhitelistedRoles = entityServiceWhitelistedRoles;
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

        [HttpPost]
        public async Task<IActionResult> UpdateLinkFilter(ulong guildId, ModerationLink vm)
        {
            var currentSettings = await _moderationSettings.GetSettingsByGuild(guildId);

            #region whitelisted channels link

            var currentWhitelistedChannels = currentSettings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Link);

            var channelsToRemove = new List<WhitelistedChannel>();

            var channelsToAdd = new List<WhitelistedChannel>();

            foreach (var channelId in vm.WhitelistedChannels)
            {
                if (!currentWhitelistedChannels.Any(c => c.ChannelId == channelId))
                {
                    channelsToAdd.Add(new WhitelistedChannel
                    {
                        ChannelId = channelId,
                        GuildId = guildId,
                        Moderationsettings = currentSettings,
                        WhitelistType = WhitelistType.Link
                    });
                }
            }

            foreach (var channel in currentWhitelistedChannels)
            {
                if (!vm.WhitelistedChannels.Any(c => c == channel.ChannelId))
                {
                    channelsToRemove.Add(channel);
                }
            }

            await _entityServiceWhitelistedChannels.CreateBulk(channelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(channelsToRemove);

            #endregion

            #region whitelisted roles link

            var currentWhitelistedRoles = currentSettings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Link);

            var rolesToRemove = new List<WhitelistedRole>();

            var rolesToAdd = new List<WhitelistedRole>();

            foreach (var roleId in vm.WhitelistedRoles)
            {
                if (!currentWhitelistedRoles.Any(c => c.RoleId == roleId))
                {
                    rolesToAdd.Add(new WhitelistedRole
                    {
                        GuildId = guildId,
                        RoleId = roleId,
                        Moderationsettings = currentSettings,
                        WhitelistType = WhitelistType.Link
                    });
                }
            }

            foreach (var role in currentWhitelistedRoles)
            {
                if (!vm.WhitelistedRoles.Any(r => r == role.RoleId))
                {
                    rolesToRemove.Add(role);
                }
            }

            await _entityServiceWhitelistedRoles.CreateBulk(rolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(rolesToRemove);

            #endregion

            var filter = currentSettings.LinkFilter;

            filter.Enabled = vm.Enabled;

            filter.WarningExpirePeriod = vm.WarningExpirePeriod;

            await _entityServiceLinkFilter.Update(filter);

            // Clearing cache prompts the module to refetch moderation settings from the database the next time it needs them, essentially updating them.
            _moderationSettings.ClearCacheByGuild(vm.GuildId);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfanityFilter(ulong guildId, ModerationProfanity vm)
        {

        }
    }
}