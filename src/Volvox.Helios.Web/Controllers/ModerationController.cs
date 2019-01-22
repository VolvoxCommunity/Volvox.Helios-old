using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Core.Modules.ModerationModule.WarningService;
using Volvox.Helios.Core.Utilities.Constants;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.Models.Moderation;
using Volvox.Helios.Web.ViewModels.Moderation;

namespace Volvox.Helios.Web.Controllers
{
    // TODO : Find more efficient way of making all those db calls. No way of batching them currently.
    // TODO : Instead of clearing cache through filter, find way to make either module settings save navigation properties, or make entity service clear module settings.
    [Authorize]
    [Route("/Moderation/{guildId}")]
    [IsUserGuildAdminFilter]
    [EnsureModerationSettingsEntryExistsFilter]
    [ClearCacheModerationSettings]
    public class ModerationController : Controller
    {
        #region private vars
        private readonly IModuleSettingsService<ModerationSettings> _moderationSettings;

        private readonly IEntityService<ProfanityFilter> _entityServiceProfanityFilter;

        private readonly IEntityService<LinkFilter> _entityServiceLinkFilter;

        private readonly IEntityService<WhitelistedChannel> _entityServiceWhitelistedChannels;

        private readonly IEntityService<WhitelistedRole> _entityServiceWhitelistedRoles;

        private readonly IEntityService<WhitelistedLink> _entityServiceWhitelistedLinks;

        private readonly IEntityService<BannedWord> _entityServiceBannedWords;

        private readonly IEntityService<Punishment> _entityServicePunishments;

        private readonly IEntityService<Warning> _entityServiceWarnings;

        private readonly IEntityService<ActivePunishment> _entityServiceActivePunishments;

        private readonly IEntityService<UserWarnings> _entityServiceUsers;

        private readonly IDiscordUserService _discordUserService;

        private readonly IPunishmentService _punishmentService;

        private readonly IWarningService _warningService;

        private readonly IModerationModuleUtils _moderationModuleUtils;
        #endregion

        public ModerationController(IModuleSettingsService<ModerationSettings> moderationSettings,
            IEntityService<ProfanityFilter> entityServiceProfanityFilter,
            IEntityService<LinkFilter> entityServiceLinkFilter,
            IEntityService<WhitelistedChannel> entityServiceWhitelistedChannels,
            IEntityService<WhitelistedRole> entityServiceWhitelistedRoles,
            IEntityService<WhitelistedLink> entityServiceWhitelistedLinks,
            IEntityService<BannedWord> entityServiceBannedWords,
            IEntityService<Punishment> entityServicePunishments,
            IEntityService<Warning> entityServiceWarnings,
            IEntityService<ActivePunishment> entityServiceActivePunishments,
            IEntityService<UserWarnings> entityServiceUsers,
            IDiscordUserService discordUserService,
            IPunishmentService punishmentService,
            IWarningService warningService,
            IModerationModuleUtils moderationModuleUtils
        )
        {
            _moderationSettings = moderationSettings;

            _entityServiceProfanityFilter = entityServiceProfanityFilter;

            _entityServiceLinkFilter = entityServiceLinkFilter;

            _entityServiceWhitelistedChannels = entityServiceWhitelistedChannels;

            _entityServiceWhitelistedRoles = entityServiceWhitelistedRoles;

            _entityServiceWhitelistedLinks = entityServiceWhitelistedLinks;

            _entityServiceBannedWords = entityServiceBannedWords;

            _entityServicePunishments = entityServicePunishments;

            _entityServiceWarnings = entityServiceWarnings;

            _entityServiceActivePunishments = entityServiceActivePunishments;

            _entityServiceUsers = entityServiceUsers;

            _discordUserService = discordUserService;

            _punishmentService = punishmentService;

            _warningService = warningService;

            _moderationModuleUtils = moderationModuleUtils;
        }

        private void ClearCacheById(ulong guildId)
        {
            _moderationSettings.GetSettingsByGuild(guildId);
        }

        [HttpGet]
        public async Task<IActionResult> Index(ulong guildId)
        {
            return View();
        }

        [HttpGet("general")]
        public async Task<IActionResult> General(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(guildId);

            var guildChannels = await guildService.GetChannels(guildId);

            var textChannels = guildChannels.Where(c => c.Type == (int)ChannelType.Text);

            var roles = await guildService.GetRoles(guildId);

            var alreadyWhitelistedRoles = settings?.WhitelistedRoles.Where(r => r.WhitelistType == FilterType.Global).Select(r => r.RoleId);

            var alreadyWhitelistedChannels = settings?.WhitelistedChannels.Where(r => r.WhitelistType == FilterType.Global).Select(c => c.ChannelId);

            var vm = new GlobalSettingsViewModel
            {
                Enabled = settings?.Enabled ?? false,
                WhitelistedChannels = new MultiSelectList(textChannels, "Id", "Name", alreadyWhitelistedChannels),
                WhitelistedRoles = new MultiSelectList(roles, "Id", "Name", alreadyWhitelistedRoles)
            };

            return View("GlobalSettings", vm);
        }

        [HttpPost("general")]
        public async Task<IActionResult> General(ulong guildId, GlobalSettingsViewModel vm)
        {
            var currentSettings = await _moderationModuleUtils.GetModerationSettings(guildId);

            currentSettings.Enabled = vm.Enabled;

            var newChannelState = GetNewChannelState(currentSettings, FilterType.Global, vm.SelectedChannels);

            var newRolesState = GetNewRolesState(currentSettings, FilterType.Global, vm.SelectedRoles);

            await _entityServiceWhitelistedChannels.CreateBulk(newChannelState.ChannelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(newChannelState.ChannelsToRemove);

            await _entityServiceWhitelistedRoles.CreateBulk(newRolesState.RolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(newRolesState.RolesToRemove);

            await _moderationSettings.SaveSettings(currentSettings);

            return RedirectToAction("general");
        }

        [HttpGet("linkfilter")]
        public async Task<IActionResult> LinkFilter(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(guildId);

            var guildChannels = await guildService.GetChannels(guildId);

            var textChannels = guildChannels?.Where(c => c.Type == (int)ChannelType.Text);

            var roles = await guildService.GetRoles(guildId);

            var alreadyWhitelistedRoles = settings?.WhitelistedRoles.Where(r => r.WhitelistType == FilterType.Link).Select(r => r.RoleId);

            var alreadyWhitelistedChannels = settings?.WhitelistedChannels.Where(r => r.WhitelistType == FilterType.Link).Select(c => c.ChannelId);

            var links = settings.LinkFilter?.WhitelistedLinks.Select(l => l.Link) ?? new List<string>();

            var vm = new LinkFilterViewModel
            {
                Enabled = settings?.LinkFilter?.Enabled ?? false,
                WhitelistedChannels = new MultiSelectList(textChannels, "Id", "Name", alreadyWhitelistedChannels),
                WhitelistedRoles = new MultiSelectList(roles, "Id", "Name", alreadyWhitelistedRoles),
                WhitelistedLinks = new MultiSelectList(links, links),
                WarningExpirePeriod = settings?.LinkFilter?.WarningExpirePeriod ?? 0
            };

            return View(vm);
        }

        [HttpPost("linkfilter")]
        public async Task<IActionResult> UpdateLinkFilter(ulong guildId, LinkFilterViewModel vm)
        {
            var currentSettings = await _moderationModuleUtils.GetModerationSettings(guildId);

            var filter = await _entityServiceLinkFilter.GetFirst(f => f.GuildId == guildId);

            var createNewFilter = false;

            if (filter is null)
            {
                filter = new LinkFilter { GuildId = guildId };

                createNewFilter = true;
            }

            filter.Enabled = vm.Enabled;

            filter.WarningExpirePeriod = vm.WarningExpirePeriod;

            if (createNewFilter)
            {
                await _entityServiceLinkFilter.Create(filter);
            }
            else
            {
                await _entityServiceLinkFilter.Update(filter);
            }

            var newChannelState = GetNewChannelState(currentSettings, FilterType.Link, vm.SelectedChannels);

            var newRolesState = GetNewRolesState(currentSettings, FilterType.Link, vm.SelectedRoles);

            var newLinkState = GetNewLinksState(currentSettings, vm.SelectedLinks );

            await _entityServiceWhitelistedChannels.CreateBulk(newChannelState.ChannelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(newChannelState.ChannelsToRemove);

            await _entityServiceWhitelistedRoles.CreateBulk(newRolesState.RolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(newRolesState.RolesToRemove);

            await _entityServiceWhitelistedLinks.CreateBulk(newLinkState.LinksToAdd);

            await _entityServiceWhitelistedLinks.RemoveBulk(newLinkState.LinksToRemove);

            return RedirectToAction("linkfilter");
        }

        [HttpGet("profanityfilter")]
        public async Task<IActionResult> ProfanityFilter(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(guildId);

            var guildChannels = await guildService.GetChannels(guildId);

            var textChannels = guildChannels?.Where(c => c.Type == (int)ChannelType.Text);

            var roles = await guildService.GetRoles(guildId);

            var alreadyWhitelistedRoles = settings?.WhitelistedRoles.Where(r => r.WhitelistType == FilterType.Profanity).Select(r => r.RoleId);

            var alreadyWhitelistedChannels = settings?.WhitelistedChannels.Where(r => r.WhitelistType == FilterType.Profanity).Select(c => c.ChannelId);

            var words = settings?.ProfanityFilter?.BannedWords.Select(w => w.Word) ?? new List<string>();

            var vm = new ProfanityFilterViewModel
            {
                Enabled = settings?.ProfanityFilter?.Enabled ?? false,
                WhitelistedChannels = new MultiSelectList(textChannels, "Id", "Name", alreadyWhitelistedChannels),
                WhitelistedRoles = new MultiSelectList(roles, "Id", "Name", alreadyWhitelistedRoles),
                BannedWords = new MultiSelectList(words, words),
                UseDefaultBannedWords = settings?.ProfanityFilter?.UseDefaultList ?? true,
                WarningExpirePeriod = settings?.ProfanityFilter?.WarningExpirePeriod ?? 0
            };
            
            return View(vm);
        }

        [HttpPost("profanityfilter")]
        public async Task<IActionResult> UpdateProfanityFilter(ulong guildId, ProfanityFilterViewModel vm)
        {
            var currentSettings = await _moderationModuleUtils.GetModerationSettings(guildId);

            var filter = await _entityServiceProfanityFilter.GetFirst(f => f.GuildId == guildId);

            var createNewFilter = false;

            if (filter is null)
            {
                filter = new ProfanityFilter { GuildId = guildId };

                createNewFilter = true;
            }

            filter.Enabled = vm.Enabled;

            filter.WarningExpirePeriod = vm.WarningExpirePeriod;

            filter.UseDefaultList = vm.UseDefaultBannedWords;

            if (createNewFilter)
            {
                await _entityServiceProfanityFilter.Create(filter);
            }
            else
            {
                await _entityServiceProfanityFilter.Update(filter);
            }

            var newChannelState = GetNewChannelState(currentSettings, FilterType.Profanity, vm.SelectedChannels);

            var newRolesState = GetNewRolesState(currentSettings, FilterType.Profanity, vm.SelectedRoles);

            var newBannedWordsState = GetNewBannedWordsState(currentSettings, vm.SelectedWords);

            await _entityServiceWhitelistedChannels.CreateBulk(newChannelState.ChannelsToAdd);

            await _entityServiceWhitelistedChannels.RemoveBulk(newChannelState.ChannelsToRemove);

            await _entityServiceWhitelistedRoles.CreateBulk(newRolesState.RolesToAdd);

            await _entityServiceWhitelistedRoles.RemoveBulk(newRolesState.RolesToRemove);

            await _entityServiceBannedWords.CreateBulk(newBannedWordsState.WordsToAdd);

            await _entityServiceBannedWords.RemoveBulk(newBannedWordsState.WordsToRemove);

            return RedirectToAction("profanityfilter");
        }

        [HttpGet("punishments")]
        public async Task<IActionResult> Punishments (ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            var punishments = await _entityServicePunishments.Get(x => x.GuildId == guildId);

            var punishmentModels = new List<PunishmentModel>();

            var roles = await guildService.GetRoles(guildId);

            foreach (var p in punishments)
            {
                var role = roles.FirstOrDefault(r => r.Id == p.RoleId);

                punishmentModels.Add(new PunishmentModel
                {
                    PunishDuration = p.PunishDuration,
                    PunishType = p.PunishType,
                    RoleId = p.RoleId,
                    WarningThreshold = p.WarningThreshold,
                    WarningType = p.WarningType,
                    Role = new SelectList(roles, "Id", "Name", role),
                    DeletePunishment = false,
                    PunishmentId = p.Id
                });
            }

            var vm = new PunishmentsViewModel
            {
                Punishments = punishmentModels
            };
            return View(vm);
        }

        [HttpPost("punishments")]
        public async Task<IActionResult> Punishments(ulong guildId, PunishmentsViewModel vm)
        {
            var punishments = vm.Punishments ?? new List<PunishmentModel>();

            var punishmentsToRemove = new List<Punishment>();

            foreach (var model in punishments.Where(x => x.DeletePunishment))
            {
                if (model.PunishmentId.HasValue)
                {
                    punishmentsToRemove.Add(ConvertModelPunishment(guildId, model));
                }
            }

            await _entityServicePunishments.RemoveBulk(punishmentsToRemove);

            return RedirectToAction("punishments");
        }

        [HttpGet("newpunishment")]
        public async Task<IActionResult> NewPunishment(ulong guildId, [FromServices] IDiscordGuildService guildService)
        {
            var roles = await guildService.GetRoles(guildId);

            return View(new PunishmentModel {
                Role = new SelectList(roles, "Id", "Name")
            });
        }

        [HttpPost("newpunishment")]
        public async Task<IActionResult> NewPunishment(ulong guildId, PunishmentModel vm)
        {
            if (ModelState.IsValid)
            {
                var punishment = ConvertModelPunishment(guildId, vm);

                await _entityServicePunishments.Create(punishment);
            }

            return RedirectToAction("punishments");
        }

        [HttpGet("users/{pageNo}"), HttpGet("users")]
        public IActionResult Users(ulong guildId, [FromServices] IDiscordGuildService guildService, int pageNo = 0)
        {
            var allUsers = _discordUserService.GetUsers(guildId);

            var totalPageCount = ( allUsers.Count + ModuleConstants.ResultsPerPageUsers ) / ModuleConstants.ResultsPerPageUsers;

            var usersToDisplay = _discordUserService.GetUsers(guildId).Where(u => !u.IsBot)
                .Skip(pageNo * ModuleConstants.ResultsPerPageUsers).Take(ModuleConstants.ResultsPerPageUsers).Select(u => new UserModel
            {
                Id = u.Id,
                Username = $"{u.Username}#{u.Discriminator}",
                AvatarUrl = u.GetAvatarUrl(size: ModuleConstants.AvatarSizeUsers) ?? u.GetDefaultAvatarUrl()
            }).ToList();

            var vm = new UsersViewModel
            {
                Users = usersToDisplay,
                PageNo = pageNo,
                TotalPageCount = totalPageCount
            };

            return View(vm);
        }
      
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> User(ulong guildId, ulong userId, [FromServices] IDiscordUserService userService)
        {
            var user = await _entityServiceUsers.GetFirst(u => u.UserId == userId, x => x.Warnings, x => x.ActivePunishments);

            var socketUser = await userService.GetUser(userId);

            // Create user entry if one doesn't exist.
            if (user == null)
            {
                await _entityServiceUsers.Create(new UserWarnings { UserId = userId, GuildId = guildId });

                ClearCacheById(guildId);
            }
               
            var vm = new UserViewModel
            {
                ActivePunishments = user?.ActivePunishments ?? new List<ActivePunishment>(),
                Warnings = user?.Warnings ?? new List<Warning>(),
                UserId = socketUser.Id,
                Username = socketUser.Username,
                Discriminator = socketUser.Discriminator.ToString()
            };

            return View(vm);
        }

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> User(ulong guildId, ulong userId, UserViewModel vm) 
        {
            var punishmentsToRemoveIds = vm.ActivePunishments.Where(p => p.Remove).Select(p => p.Id);

            var punishmentsToRemove = await _entityServiceActivePunishments.Get(p => punishmentsToRemoveIds.Contains(p.Id)
            && p.User.UserId == userId && p.User.GuildId == guildId, p => p.User);

            var warningsToRemoveIds = vm.Warnings.Where(w => w.Remove).Select(w => w.Id);

            var warningsToRemove = await _entityServiceWarnings.Get(w => warningsToRemoveIds.Contains(w.Id)
            && w.User.UserId == userId && w.User.GuildId == guildId, w => w.User);

            await _punishmentService.RemovePunishmentBulk(punishmentsToRemove);

            await _warningService.RemoveWarningBulk(warningsToRemove);

            return RedirectToAction("User");
        }

        private Punishment ConvertModelPunishment(ulong guildId, PunishmentModel model)
        {
            var punishment = new Punishment
            {
                GuildId = guildId,
                PunishDuration = model.PunishDuration,
                PunishType = model.PunishType,
                WarningThreshold = model.WarningThreshold,
                WarningType = model.WarningType,
                RoleId = model.PunishType == PunishType.AddRole ? model.RoleId : null
            };

            if (model.PunishmentId.HasValue)
            {
                punishment.Id = model.PunishmentId.Value;
            }

            return punishment;
        }

        private NewChannelsState GetNewChannelState(ModerationSettings currentSettings, FilterType type, List<ulong> channelIds)
        {
            var currentWhitelistedChannels = currentSettings?.WhitelistedChannels.Where(c => c.WhitelistType == type);

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

        private NewRolesState GetNewRolesState(ModerationSettings currentSettings, FilterType type, List<ulong> roleIds)
        {
            var currentWhitelistedRoles = currentSettings?.WhitelistedRoles.Where(r => r.WhitelistType == type);

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

            var whitelistedLinks = currentSettings?.LinkFilter?.WhitelistedLinks ?? new List<WhitelistedLink>();

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

            var bannedWords = currentSettings?.ProfanityFilter?.BannedWords ?? new List<BannedWord>();

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

    internal class NewBannedWordsState
    {
        public List<BannedWord> WordsToAdd { get; set; }

        public List<BannedWord> WordsToRemove { get; set; }
    }

    internal class NewChannelsState
    {
        public List<WhitelistedChannel> ChannelsToAdd { get; set; }

        public List<WhitelistedChannel> ChannelsToRemove { get; set; }
    }

    internal class NewWhitelistedLinksState
    {
        public List<WhitelistedLink> LinksToAdd { get; set; }

        public List<WhitelistedLink> LinksToRemove { get; set; }
    }

    internal class NewRolesState
    {
        public List<WhitelistedRole> RolesToAdd { get; set; }

        public List<WhitelistedRole> RolesToRemove { get; set; }
    }
}