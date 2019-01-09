using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.BypassCheck
{
    public class BypassCheck : IBypassCheck
    {
        public bool HasBypassAuthority(ModerationSettings settings, SocketMessage message, FilterType type)
        {
            var whitelistedChannels = settings?.WhitelistedChannels.Where(c => c.WhitelistType == type);

            var whitelistedRoles = settings?.WhitelistedRoles.Where(r => r.WhitelistType == type);

            var whitelistedFields = new WhitelistedFields
            {
                WhitelistedChannels = whitelistedChannels,
                WhitelistedRoles = whitelistedRoles
            };

            var author = message.Author as SocketGuildUser;

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
