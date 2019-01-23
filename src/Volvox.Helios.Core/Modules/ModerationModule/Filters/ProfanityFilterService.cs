using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Volvox.Helios.Core.Modules.ModerationModule.BypassCheck;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public class ProfanityFilterService : IFilterService
    {
        private readonly IBypassCheck _bypassCheck;

        private readonly List<string> _defaultBannedWords = new List<string>();

        private readonly IModerationModuleUtils _moderationModuleUtils;

        public ProfanityFilterService(IBypassCheck bypassCheck,
            IConfiguration config, IModerationModuleUtils moderationModuleUtils)
        {
            _bypassCheck = bypassCheck;

            _moderationModuleUtils = moderationModuleUtils;

            _defaultBannedWords.AddRange(config.GetSection("BannedWords").GetChildren().Select(x => x.Value));
        }

        /// <inheritdoc />
        public async Task<bool> CheckViolation(SocketMessage message)
        {
            var settings =
                await _moderationModuleUtils.GetModerationSettings(( (SocketGuildUser)message.Author ).Guild.Id);

            var filterViolatedFlag = false;

            if (!await HasBypassAuthority(message) && settings.ProfanityFilter != null)
                if (ContainsProfanity(settings.ProfanityFilter, message.Content))
                    filterViolatedFlag = true;

            return filterViolatedFlag;
        }

        /// <inheritdoc />
        public FilterMetaData GetFilterMetaData()
        {
            return new FilterMetaData(FilterType.Profanity);
        }

        /// <inheritdoc />
        public async Task<int> GetWarningExpirePeriod(ulong guildId)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(guildId);

            return settings.ProfanityFilter.WarningExpirePeriod;
        }

        private bool ContainsProfanity(ProfanityFilter profanityFilter, string message)
        {
            // Normalize message to lowercase and split into array of words.
            var messageWords = message.ToLower().Split(" ");

            var bannedWords = profanityFilter.BannedWords.Select(w => w.Word).ToList();

            // Check for default banned words if UserDefaultList enabled.
            if (profanityFilter.UseDefaultList)
                bannedWords.AddRange(_defaultBannedWords);

            foreach (var word in messageWords)
            foreach (var bannedWord in bannedWords)
                if (word == bannedWord)
                    return true;

            return false;
        }

        // TODO : Future feature. Method currently unused for now.
        private string ConvertClbuttic(string word)
        {
            return word.Replace("[a]", "[a A @]")
                .Replace("[b]", "[b B I3 l3 i3]")
                .Replace("[c]", "(?:[c C \\(]|[k K])")
                .Replace("[d]", "[d D]")
                .Replace("[e]", "[e E 3]")
                .Replace("[f]", "(?:[f F]|[ph pH Ph PH])")
                .Replace("[g]", "[g G 6]")
                .Replace("[h]", "[h H]")
                .Replace("[i]", "[i I l ! 1]")
                .Replace("[j]", "[j J]")
                .Replace("[k]", "(?:[c C \\(]|[k K])")
                .Replace("[l]", "[l L 1 ! i]")
                .Replace("[m]", "[m M]")
                .Replace("[n]", "[n N]")
                .Replace("[o]", "[o O 0]")
                .Replace("[p]", "[p P]")
                .Replace("[q]", "[q Q 9]")
                .Replace("[r]", "[r R]")
                .Replace("[s]", "[s S $ 5]")
                .Replace("[t]", "[t T 7]")
                .Replace("[u]", "[u U v V]")
                .Replace("[v]", "[v V u U]")
                .Replace("[w]", "[w W vv VV]")
                .Replace("[x]", "[x X]")
                .Replace("[y]", "[y Y]");
        }

        private async Task<bool> HasBypassAuthority(SocketMessage message)
        {
            return await _bypassCheck.HasBypassAuthority(message, FilterType.Profanity);
        }
    }
}