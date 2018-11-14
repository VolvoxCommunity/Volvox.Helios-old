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
using Volvox.Helios.Web.ViewModels.Moderation;
using Volvox.Helios.Service.Discord.Guild;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Discord;
using Discord;

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


        [HttpGet("linkfilter")]
        public async Task<IActionResult> LinkFilter(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            var settings = await _moderationSettings.GetSettingsByGuild(guildId, s => s.LinkFilter.WhitelistedLinks);

            var guildChannels = await guildService.GetChannels(guildId);

            var textChannels = guildChannels.Where(c => c.Type == (int)ChannelType.Text);

            var roles = await guildService.GetRoles(guildId);

            var vm = new LinkFilterViewModel
            {
                Enabled = settings.LinkFilter.Enabled,
                WhitelistedChannels = new MultiSelectList(textChannels, "Id", "Name"),
                WhitelistedRoles = new MultiSelectList(roles, "Id", "Name"),
                WhitelistedLinks = new List<string>() { "test1", "test2", "test3" }
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> LinkFilter(ulong guildId, ModerationLink vm)
        {
            var currentSettings = await _moderationSettings.GetSettingsByGuild(guildId);

            #region general filter settings link

            var filter = currentSettings.LinkFilter;

            // TODO : null check. if null, create as necessary.

            filter.Enabled = vm.Enabled;

            filter.WarningExpirePeriod = vm.WarningExpirePeriod;

            await _entityServiceLinkFilter.Update(filter);

            #endregion

            #region whitelisted channels link

            var newChannelState = GetNewChannelState(currentSettings, WhitelistType.Link, vm.WhitelistedChannels);

            await _entityServiceWhitelistedChannels.CreateBulk(newChannelState.ChannelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(newChannelState.ChannelsToRemove);

            #endregion

            #region whitelisted roles link

            var newRolesState = GetNewRolesState(currentSettings, WhitelistType.Link, vm.WhitelistedRoles);

            await _entityServiceWhitelistedRoles.CreateBulk(newRolesState.RolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(newRolesState.RolesToRemove);

            #endregion

            // Clearing cache prompts the module to refetch moderation settings from the database the next time it needs them, essentially updating them.
            _moderationSettings.ClearCacheByGuild(vm.GuildId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfanityFilter(ulong guildId, ModerationProfanity vm)
        {
            var currentSettings = await _moderationSettings.GetSettingsByGuild(guildId);

            #region general filter settings profanity

            var filter = currentSettings.ProfanityFilter;

            // TODO : null check. if null, create as necessary.

            filter.Enabled = vm.Enabled;

            filter.WarningExpirePeriod = vm.WarningExpirePeriod;

            await _entityServiceProfanityFilter.Update(filter);

            #endregion

            #region whitelisted channels profanity

            var newChannelState = GetNewChannelState(currentSettings, WhitelistType.Profanity, vm.WhitelistedChannels);

            await _entityServiceWhitelistedChannels.CreateBulk(newChannelState.ChannelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(newChannelState.ChannelsToRemove);

            #endregion

            #region whitelisted roles profanity

            var newRolesState = GetNewRolesState(currentSettings, WhitelistType.Profanity, vm.WhitelistedRoles);
            
            await _entityServiceWhitelistedRoles.CreateBulk(newRolesState.RolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(newRolesState.RolesToRemove);

            #endregion

            // Clearing cache prompts the module to refetch moderation settings from the database the next time it needs them, essentially updating them.
            _moderationSettings.ClearCacheByGuild(vm.GuildId);

            return View(vm);
        }

        private NewChannelsState GetNewChannelState(ModerationSettings currentSettings, WhitelistType type, List<ulong> channelIds)
        {
            var currentWhitelistedChannels = currentSettings.WhitelistedChannels.Where(c => c.WhitelistType == type);

            var channelsToAdd = new List<WhitelistedChannel>();

            var channelsToRemove = new List<WhitelistedChannel>();

            foreach (var channelId in channelIds)
            {
                if (!currentWhitelistedChannels.Any(c => c.ChannelId == channelId))
                {
                    channelsToAdd.Add(new WhitelistedChannel
                    {
                        ChannelId = channelId,
                        GuildId = currentSettings.GuildId,
                        Moderationsettings = currentSettings,
                        WhitelistType = type
                    });
                }
            }

            foreach (var channel in currentWhitelistedChannels)
            {
                if (!channelIds.Any(c => c == channel.ChannelId))
                {
                    channelsToRemove.Add(channel);
                }
            }

            return new NewChannelsState
            {
                ChannelsToAdd = channelsToAdd,
                ChannelsToRemove = channelsToRemove
            };
        }

        private NewRolesState GetNewRolesState(ModerationSettings currentSettings, WhitelistType type, List<ulong> roleIds)
        {
            var currentWhitelistedRoles = currentSettings.WhitelistedRoles.Where(r => r.WhitelistType == type);

            var rolesToRemove = new List<WhitelistedRole>();

            var rolesToAdd = new List<WhitelistedRole>();

            foreach (var roleId in roleIds)
            {
                if (!currentWhitelistedRoles.Any(c => c.RoleId == roleId))
                {
                    rolesToAdd.Add(new WhitelistedRole
                    {
                        GuildId = currentSettings.GuildId,
                        RoleId = roleId,
                        Moderationsettings = currentSettings,
                        WhitelistType = type
                    });
                }
            }

            foreach (var role in currentWhitelistedRoles)
            {
                if (!roleIds.Any(r => r == role.RoleId))
                {
                    rolesToRemove.Add(role);
                }
            }

            return new NewRolesState
            {
                RolesToAdd = rolesToAdd,
                RolesToRemove = rolesToRemove
            };
        }
    }

    class NewChannelsState
    {
        public List<WhitelistedChannel> ChannelsToAdd { get; set; }

        public List<WhitelistedChannel> ChannelsToRemove { get; set; }
    }

    class NewRolesState
    {
        public List<WhitelistedRole> RolesToAdd { get; set; }

        public List<WhitelistedRole> RolesToRemove { get; set; }
    }
}