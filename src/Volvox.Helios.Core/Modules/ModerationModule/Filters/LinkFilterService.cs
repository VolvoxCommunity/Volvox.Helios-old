using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.ModerationModule.BypassCheck;
using Volvox.Helios.Core.Modules.ModerationModule.ViolationService;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters.Link
{
    public class LinkFilterService : IFilterService<LinkFilter>
    {
        private readonly IViolationService _violationService;

        private readonly IBypassCheck _bypassCheck;

        public LinkFilterService(IViolationService violationService, IBypassCheck bypassCheck)
        {
            _violationService = violationService;

            _bypassCheck = bypassCheck;
        }

        public bool CheckViolation(ModerationSettings settings, SocketMessage message)
        {
            var filterViolatedFlag = false;

            if (!HasBypassAuthority(settings, message) && settings.LinkFilter != null)
            {
                if (ContainsIllegalLink(settings.LinkFilter, message))
                    filterViolatedFlag = true;
            }

            return filterViolatedFlag;
        }

        private bool ContainsIllegalLink(LinkFilter linkFilter, SocketMessage message)
        {
            // Normalize message to lowercase and split into array of words.
            var messageWords = message.Content.ToLower().Split(" ");

            var whitelistedLinks = linkFilter.WhitelistedLinks.Select(l => l.Link);

            // Regular expression for detecting url patterns
            var urlCheck = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");

            // Flag for tracking whether current url is whitelisted
            var isLinkLegal = false;

            // Check each word for illegal link
            foreach (var word in messageWords)
            {
                // TODO: In future version of this module, check if url entered is of the same base, and not just matches exactly.
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

        public async Task HandleViolation(ModerationSettings settings, SocketMessage message)
        {
            await _violationService.HandleViolation(settings, message, WarningType.Link);
        }

        private bool HasBypassAuthority(ModerationSettings settings, SocketMessage message)
        {
            return _bypassCheck.HasBypassAuthority(settings, message, WhitelistType.Link);
        }
    }
}
