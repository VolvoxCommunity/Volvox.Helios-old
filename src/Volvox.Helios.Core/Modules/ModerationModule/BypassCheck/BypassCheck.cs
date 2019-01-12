using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.BypassCheck
{
    public class BypassCheck : IBypassCheck
    {
        private readonly IModerationModuleUtils _moderationModuleUtils;

        public BypassCheck(IModerationModuleUtils moderationModuleUtils)
        {
            _moderationModuleUtils = moderationModuleUtils;
        }

        /// <inheritdoc />
        public async Task<bool> HasBypassAuthority(SocketMessage message, FilterType type)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings((( SocketGuildUser)message.Author  ).Guild.Id);

            var whitelistedChannels = settings?.WhitelistedChannels.Where(c => c.WhitelistType == type);

            var whitelistedRoles = settings?.WhitelistedRoles.Where(r => r.WhitelistType == type);

            var whitelistedFields = new WhitelistedFields
            {
                WhitelistedChannels = whitelistedChannels,
                WhitelistedRoles = whitelistedRoles
            };

            var author = (SocketGuildUser)message.Author;

            var channelPostedId = message.Channel.Id;

            return CheckWhitelisted(author, channelPostedId, whitelistedFields);
        }

        private bool CheckWhitelisted(SocketGuildUser author, ulong postedChannelId,
            WhitelistedFields fields)
        {
            // Bots bypass check.
            if (author.IsBot)
                return true;

            // Check if channel id is whitelisted.
            if (fields.WhitelistedChannels.Any(x => x.ChannelId == postedChannelId))
                return true;

            // Check for whitelisted role.
            if (author.Roles.Any(r => fields.WhitelistedRoles.Any(w => w.RoleId == r.Id)))
                return true;

            return false;
        }

        internal struct WhitelistedFields
        {
            public IEnumerable<WhitelistedChannel> WhitelistedChannels;

            public IEnumerable<WhitelistedRole> WhitelistedRoles;
        }
    }
}
