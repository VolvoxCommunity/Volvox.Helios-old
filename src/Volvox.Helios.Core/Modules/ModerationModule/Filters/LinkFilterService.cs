using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.ModerationModule.BypassCheck;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Core.Modules.ModerationModule.ViolationService;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters.Link
{
    public class LinkFilterService : IFilterService
    {
        private readonly IViolationService _violationService;

        private readonly IBypassCheck _bypassCheck;

        private readonly IModerationModuleUtils _moderationModuleUtils;

        public LinkFilterService(IViolationService violationService, IBypassCheck bypassCheck,
            IModerationModuleUtils moderationModuleUtils)
        {
            _violationService = violationService;

            _bypassCheck = bypassCheck;

            _moderationModuleUtils = moderationModuleUtils;
        }

        public async Task<bool> CheckViolation(SocketMessage message)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(( message.Author as SocketGuildUser ).Guild.Id);

            var filterViolatedFlag = false;

            if (!await HasBypassAuthority(message) && settings.LinkFilter != null)
            {
                if (ContainsIllegalLink(settings.LinkFilter, message.Content))
                    filterViolatedFlag = true;
            }

            return filterViolatedFlag;
        }

        private bool ContainsIllegalLink(LinkFilter linkFilter, string message)
        {
            // Normalize message to lowercase and split into array of words.
            var messageWords = message.ToLower().Split(" ");

            var whitelistedLinks = linkFilter.WhitelistedLinks.Select(l => l.Link);

            // Regular expression for detecting url patterns
            var urlCheck = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");

            // Flag for tracking whether current url is whitelisted
            var isLinkLegal = false;

            // Check each word for illegal link
            foreach (var word in messageWords)
            {
                if (urlCheck.IsMatch(word))
                {
                    foreach (var link in whitelistedLinks)
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
            }
            return false;
        }

        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        public async Task HandleViolation(SocketMessage message)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings((message.Author as SocketGuildUser).Guild.Id);

            await _violationService.HandleViolation(message, FilterType.Link);
        }

        private async Task<bool> HasBypassAuthority(SocketMessage message)
        {
            return await _bypassCheck.HasBypassAuthority(message, FilterType.Link);
        }

        public FilterType GetFilterType()
        {
            return FilterType.Link;
        }
    }
}
