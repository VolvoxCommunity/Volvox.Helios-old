using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;
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
    // TODO : ensure channel exists before adding to whitelist. same for role.

    // TODO : NULL CHECKS

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

        IEntityService<WhitelistedLink> _entityServiceWhitelistedLinks;

        IEntityService<BannedWord> _entityServiceBannedWords;

        public ModerationController(IModuleSettingsService<ModerationSettings> moderationSettings,
            IEntityService<ProfanityFilter> entityServiceProfanityFilter,
            IEntityService<LinkFilter> entityServiceLinkFilter,
            IEntityService<WhitelistedChannel> entityServiceWhitelistedChannels,
            IEntityService<WhitelistedRole> entityServiceWhitelistedRoles,
            IEntityService<WhitelistedLink> entityServiceWhitelistedLinks,
            IEntityService<BannedWord> entityServiceBannedWords)
        {
            _moderationSettings = moderationSettings;

            _entityServiceProfanityFilter = entityServiceProfanityFilter;

            _entityServiceLinkFilter = entityServiceLinkFilter;

            _entityServiceWhitelistedChannels = entityServiceWhitelistedChannels;

            _entityServiceWhitelistedRoles = entityServiceWhitelistedRoles;

            _entityServiceWhitelistedLinks = entityServiceWhitelistedLinks;

            _entityServiceBannedWords = entityServiceBannedWords;
        }

        private void ClearCacheById(ulong id)
        {
            _moderationSettings.ClearCacheByGuild(id);
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

        //[HttpPost]
        //public async Task<IActionResult> Index(ModerationGlobal vm)
        //{
        //    var settings = await _moderationSettings.GetSettingsByGuild(vm.GuildId, s => s.WhitelistedRoles, s => s.WhitelistedChannels);

        //    var newWhitelistedChannels = new List<WhitelistedChannel>();

        //    foreach (var channelId in vm.WhitelistedChannelIds)
        //    {
        //        // Channel isn't already in database, so add it. Otherwise, skip it.
        //        if (!settings.WhitelistedChannels.Any(c => c.ChannelId == channelId))
        //        {
        //            // TODO : null checks and check id is valid ulong
        //            var channel = new WhitelistedChannel() {
        //                ChannelId = channelId,
        //                GuildId = vm.GuildId,
        //                WhitelistType = WhitelistType.Global
        //            };

        //            newWhitelistedChannels.Add(channel);
        //        }
        //    }

        //    var newWhitelistedRoles = new List<WhitelistedRole>();

        //    foreach (var roleId in vm.WhitelistedRoleIds)
        //    {
        //        // Role isn't already in database, so add it. Otherwise, skip it.
        //        if (!settings.WhitelistedChannels.Any(c => c.ChannelId == roleId))
        //        {
        //            // TODO : null checks and check id is valid ulong
        //            var role = new WhitelistedRole()
        //            {
        //                GuildId = vm.GuildId,
        //                RoleId = roleId,
        //                WhitelistType = WhitelistType.Global
        //            };

        //            newWhitelistedRoles.Add(role);
        //        }
        //    }
        //    await _moderationSettings.SaveSettings(settings);

        //    // Clearing cache prompts the module to refetch moderation settings from the database the next time it needs them, essentially updating them.
        //    _moderationSettings.ClearCacheByGuild(vm.GuildId);

        //    return View();
        //}


        [HttpGet("linkfilter")]
        public async Task<IActionResult> LinkFilter(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            ClearCacheById(guildId);

            var settings = await _moderationSettings.GetSettingsByGuild(guildId, s => s.WhitelistedChannels, s => s.WhitelistedRoles, s => s.LinkFilter.WhitelistedLinks);

            var guildChannels = await guildService.GetChannels(guildId);

            var textChannels = guildChannels.Where(c => c.Type == (int)ChannelType.Text);

            var roles = await guildService.GetRoles(guildId);

            var alreadyWhitelistedRoles = settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Link).Select(r => r.RoleId).ToArray();

            var alreadyWhitelistedChannels = settings.WhitelistedChannels.Where(r => r.WhitelistType == WhitelistType.Link).Select(c => c.ChannelId).ToArray();

            var links = settings.LinkFilter.WhitelistedLinks.Select(l => l.Link);

            var vm = new LinkFilterViewModel
            {
                Enabled = settings.LinkFilter.Enabled,
                WhitelistedChannels = new MultiSelectList(textChannels, "Id", "Name", alreadyWhitelistedChannels),
                WhitelistedRoles = new MultiSelectList(roles, "Id", "Name", alreadyWhitelistedRoles),
                WhitelistedLinks = new MultiSelectList(links, links),
                WarningExpirePeriod = settings.LinkFilter.WarningExpirePeriod
            };

            ClearCacheById(guildId);

            return View(vm);
        }

        [HttpPost("linkfilter")]
        public async Task<IActionResult> LinkFilter(ulong guildId, LinkFilterViewModel vm)
        {
            ClearCacheById(guildId);

            var currentSettings = await _moderationSettings.GetSettingsByGuild(guildId, x => x.LinkFilter.WhitelistedLinks, x => x.WhitelistedChannels, x => x.WhitelistedRoles);

            var filter = await _entityServiceLinkFilter.GetFirst(f => f.GuildId == guildId);

            // TODO : null check. if null, create as necessary.

            filter.Enabled = vm.Enabled;

            filter.WarningExpirePeriod = vm.WarningExpirePeriod;

            var newChannelState = GetNewChannelState(currentSettings, WhitelistType.Link, vm.SelectedChannels);

            var newRolesState = GetNewRolesState(currentSettings, WhitelistType.Link, vm.SelectedRoles);

            var newLinkState = GetNewLinksState(currentSettings, vm.SelectedLinks );

            await _entityServiceWhitelistedChannels.CreateBulk(newChannelState.ChannelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(newChannelState.ChannelsToRemove);

            await _entityServiceWhitelistedRoles.CreateBulk(newRolesState.RolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(newRolesState.RolesToRemove);

            await _entityServiceWhitelistedLinks.CreateBulk(newLinkState.LinksToAdd);

            await _entityServiceWhitelistedLinks.RemoveBulk(newLinkState.LinksToRemove);

            await _entityServiceLinkFilter.Update(filter);

            // Clearing cache prompts the module to refetch moderation settings from the database the next time it needs them, essentially updating them.
            ClearCacheById(guildId);

            return RedirectToAction("linkfilter");
        }

        [HttpGet("profanityfilter")]
        public async Task<IActionResult> ProfanityFilter(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            ClearCacheById(guildId);

            var settings = await _moderationSettings.GetSettingsByGuild(guildId, s => s.WhitelistedChannels, s => s.WhitelistedRoles, s => s.ProfanityFilter.BannedWords);

            var guildChannels = await guildService.GetChannels(guildId);

            var textChannels = guildChannels.Where(c => c.Type == (int)ChannelType.Text);

            var roles = await guildService.GetRoles(guildId);

            var alreadyWhitelistedRoles = settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Profanity).Select(r => r.RoleId).ToArray();

            var alreadyWhitelistedChannels = settings.WhitelistedChannels.Where(r => r.WhitelistType == WhitelistType.Profanity).Select(c => c.ChannelId).ToArray();

            var words = settings.ProfanityFilter.BannedWords.Select(w => w.Word);

            var vm = new ProfanityFilterViewModel
            {
                Enabled = settings.ProfanityFilter.Enabled,
                WhitelistedChannels = new MultiSelectList(textChannels, "Id", "Name", alreadyWhitelistedChannels),
                WhitelistedRoles = new MultiSelectList(roles, "Id", "Name", alreadyWhitelistedRoles),
                BannedWords = new MultiSelectList(words, words),
                UseDefaultBannedWords = settings.ProfanityFilter.UseDefaultList,
                WarningExpirePeriod = settings.ProfanityFilter.WarningExpirePeriod
            };

            ClearCacheById(guildId);

            return View(vm);
        }

        [HttpPost("profanityfilter")]
        public async Task<IActionResult> UpdateProfanityFilter(ulong guildId, ProfanityFilterViewModel vm)
        {
            ClearCacheById(guildId);

            var currentSettings = await _moderationSettings.GetSettingsByGuild(guildId, x => x.ProfanityFilter.BannedWords, x => x.WhitelistedChannels, x => x.WhitelistedRoles);

            var filter = await _entityServiceProfanityFilter.GetFirst(f => f.GuildId == guildId);

            // TODO : null check. if null, create as necessary.

            filter.Enabled = vm.Enabled;

            filter.WarningExpirePeriod = vm.WarningExpirePeriod;

            filter.UseDefaultList = vm.UseDefaultBannedWords;

            await _entityServiceProfanityFilter.Update(filter);

            var newChannelState = GetNewChannelState(currentSettings, WhitelistType.Profanity, vm.SelectedChannels);

            var newRolesState = GetNewRolesState(currentSettings, WhitelistType.Profanity, vm.SelectedRoles);

            var newBannedWordsState = GetNewBannedWordsState(currentSettings, vm.SelectedWords);

            await _entityServiceWhitelistedChannels.CreateBulk(newChannelState.ChannelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(newChannelState.ChannelsToRemove);

            await _entityServiceWhitelistedRoles.CreateBulk(newRolesState.RolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(newRolesState.RolesToRemove);

            await _entityServiceBannedWords.CreateBulk(newBannedWordsState.WordsToAdd);

            await _entityServiceBannedWords.RemoveBulk(newBannedWordsState.WordsToRemove);

            // Clearing cache prompts the module to refetch moderation settings from the database the next time it needs them, essentially updating them.
            ClearCacheById(guildId);

            return RedirectToAction("profanityfilter");
        }

        private NewChannelsState GetNewChannelState(ModerationSettings currentSettings, WhitelistType type, List<ulong> channelIds)
        {
            var currentWhitelistedChannels = currentSettings.WhitelistedChannels.Where(c => c.WhitelistType == type);

            var channelsToAdd = new List<WhitelistedChannel>();

            var channelsToRemove = new List<WhitelistedChannel>();

            if (channelIds is null)
                channelIds = new List<ulong>();

            if (currentWhitelistedChannels is null)
                currentWhitelistedChannels = new List<WhitelistedChannel>();

            foreach (var channelId in channelIds)
            {
                if (!currentWhitelistedChannels.Any(c => c.ChannelId == channelId))
                {
                    channelsToAdd.Add(new WhitelistedChannel
                    {
                        ChannelId = channelId,
                        GuildId = currentSettings.GuildId,
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

            if (roleIds is null)
                roleIds = new List<ulong>();

            if (currentWhitelistedRoles is null)
                currentWhitelistedRoles = new List<WhitelistedRole>();

            foreach (var roleId in roleIds)
            {
                if (!currentWhitelistedRoles.Any(c => c.RoleId == roleId))
                {
                    rolesToAdd.Add(new WhitelistedRole
                    {
                        GuildId = currentSettings.GuildId,
                        RoleId = roleId,
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

        private NewWhitelistedLinksState GetNewLinksState(ModerationSettings currentSettings, HashSet<string> links)
        {
            var linksToAdd = new List<WhitelistedLink>();

            var linksToRemove = new List<WhitelistedLink>();

            var whitelistedLinks = currentSettings.LinkFilter.WhitelistedLinks ?? new List<WhitelistedLink>();

            if (links is null) links = new HashSet<string>();

            foreach (var link in links)
            {
                if (!whitelistedLinks.Any(l => l.Link == link))
                {
                    linksToAdd.Add(new WhitelistedLink
                    {
                        GuildId = currentSettings.GuildId,
                        Link = link.ToLower()
                    });
                }
            }

            foreach (var link in whitelistedLinks)
            {
                if (!links.Contains(link.Link))
                {
                    linksToRemove.Add(link);
                }
            }

            return new NewWhitelistedLinksState
            {
                LinksToAdd = linksToAdd,
                LinksToRemove = linksToRemove
            };
        }

        private NewBannedWordsState GetNewBannedWordsState(ModerationSettings currentSettings, HashSet<string> words)
        {
            var wordsToAdd = new List<BannedWord>();

            var wordsToRemove = new List<BannedWord>();

            var bannedWords = currentSettings.ProfanityFilter.BannedWords ?? new List<BannedWord>();

            if (words is null)
                words = new HashSet<string>();

            foreach (var word in words)
            {
                if (!bannedWords.Any(l => l.Word == word))
                {
                    wordsToAdd.Add(new BannedWord
                    {
                        Word = word.ToLower(),
                        GuildId = currentSettings.GuildId
                    });
                }
            }

            foreach (var word in bannedWords)
            {
                if (!words.Contains(word.Word))
                {
                    wordsToRemove.Add(word);
                }
            }

            return new NewBannedWordsState
            {
                WordsToAdd = wordsToAdd,
                WordsToRemove = wordsToRemove
            };
        }
    }

    class NewBannedWordsState
    {
        public List<BannedWord> WordsToAdd { get; set; }

        public List<BannedWord> WordsToRemove { get; set; }
    }

    class NewChannelsState
    {
        public List<WhitelistedChannel> ChannelsToAdd { get; set; }

        public List<WhitelistedChannel> ChannelsToRemove { get; set; }
    }

    class NewWhitelistedLinksState
    {
        public List<WhitelistedLink> LinksToAdd { get; set; }

        public List<WhitelistedLink> LinksToRemove { get; set; }
    }

    class NewRolesState
    {
        public List<WhitelistedRole> RolesToAdd { get; set; }

        public List<WhitelistedRole> RolesToRemove { get; set; }
    }
}