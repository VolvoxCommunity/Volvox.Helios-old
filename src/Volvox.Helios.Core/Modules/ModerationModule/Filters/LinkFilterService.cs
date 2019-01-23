using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.ModerationModule.BypassCheck;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public class LinkFilterService : IFilterService
    {
        private readonly IBypassCheck _bypassCheck;

        private readonly IModerationModuleUtils _moderationModuleUtils;

        public LinkFilterService(IBypassCheck bypassCheck,
            IModerationModuleUtils moderationModuleUtils)
        {
            _bypassCheck = bypassCheck;

            _moderationModuleUtils = moderationModuleUtils;
        }

        /// <inheritdoc />
        public async Task<bool> CheckViolation(SocketMessage message)
        {
            var settings =
                await _moderationModuleUtils.GetModerationSettings(( (SocketGuildUser)message.Author ).Guild.Id);

            var filterViolatedFlag = false;

            if (!await HasBypassAuthority(message) && settings.LinkFilter != null)
                if (ContainsIllegalLink(settings.LinkFilter, message.Content))
                    filterViolatedFlag = true;

            return filterViolatedFlag;
        }

        /// <inheritdoc />
        public async Task<int> GetWarningExpirePeriod(ulong guildId)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(guildId);

            return settings.LinkFilter.WarningExpirePeriod;
        }

        /// <inheritdoc />
        public FilterMetaData GetFilterMetaData()
        {
            return new FilterMetaData(FilterType.Link);
        }

        private bool ContainsIllegalLink(LinkFilter linkFilter, string message)
        {
            // Normalize message to lowercase and split into array of words.
            var messageWords = message.ToLower().Split(" ");

            // Regular expression for detecting url patterns
            var urlCheck = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");

            // Flag for tracking whether current url is whitelisted
            var isLinkLegal = false;

            // Check each word for illegal link
            foreach (var word in messageWords)
                if (urlCheck.IsMatch(word))
                {
                    foreach (var link in linkFilter.WhitelistedLinks.Select(l => l.Link))
                    {
                        var rgx = WildCardToRegular(link);

                        if (Regex.IsMatch(word, rgx))
                        {
                            isLinkLegal = true;
                            break;
                        }
                    }

                    if (!isLinkLegal)
                        return true;

                    isLinkLegal = false;
                }

            return false;
        }

        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private async Task<bool> HasBypassAuthority(SocketMessage message)
        {
            return await _bypassCheck.HasBypassAuthority(message, FilterType.Link);
        }
    }
}