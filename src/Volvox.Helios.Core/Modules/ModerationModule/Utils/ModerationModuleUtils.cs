using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Utils
{
    public class ModerationModuleUtils : IModerationModuleUtils
    {
        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly Dictionary<PunishType, IPunishment> _punishments = new Dictionary<PunishType, IPunishment>();

        private readonly Dictionary<FilterType, IFilterService> _filters = new Dictionary<FilterType, IFilterService>();

        public ModerationModuleUtils(IModuleSettingsService<ModerationSettings> settingsService, IList<IPunishment> punishments, IList<IFilterService> filters)
        {
            _settingsService = settingsService;

            foreach (var punishment in punishments)
            {
                var punishmentType = punishment.GetPunishmentMetaData().PunishType;

                _punishments[punishmentType] = punishment;
            }

            foreach (var filter in filters)
            {
                var filterType = filter.GetFilterMetaData().FilterType;

                _filters[filterType] = filter;
            }
        }

        public async Task<ModerationSettings> GetModerationSettings(ulong guildId)
        {
            return await _settingsService.GetSettingsByGuild(guildId, BuildSettingsQuery());
        }

        public IPunishment FetchPunishment(PunishType type)
        {
            return _punishments[type];
        }

        public IFilterService FetchFilterService(FilterType type)
        {
            return _filters[type];
        }

        private Expression<Func<ModerationSettings, object>>[] BuildSettingsQuery()
        {
            return new Expression<Func<ModerationSettings, object>>[]
            {
                s => s.Punishments,
                s => s.WhitelistedChannels,
                s => s.WhitelistedRoles,
                s => s.UserWarnings,
                s => s.ProfanityFilter.BannedWords,
                s => s.LinkFilter.WhitelistedLinks
            };
        }
    }
}
