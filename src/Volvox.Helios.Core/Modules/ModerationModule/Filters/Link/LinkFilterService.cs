using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters.Link
{
    public class LinkFilterService : ILinkFilterService
    {
        public bool LinkCheck(SocketMessage message, LinkFilter linkFilter)
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
    }
}
