using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Utils
{
    public class ModerationModuleUtils : IModerationModuleUtils
    {
        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        public ModerationModuleUtils(IModuleSettingsService<ModerationSettings> settingsService)
        {
            _settingsService = settingsService;
        }

        /// <inheritdoc />
        public async Task<ModerationSettings> GetModerationSettings(ulong guildId)
        {
            return await _settingsService.GetSettingsByGuild(guildId, BuildSettingsQuery());
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
