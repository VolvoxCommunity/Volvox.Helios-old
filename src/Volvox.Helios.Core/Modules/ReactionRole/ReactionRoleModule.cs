using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Discord;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Domain.Module;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Volvox.Helios.Service.Discord.Guild;

namespace Volvox.Helios.Core.Modules.ReactionRole
{
    public class ReactionRoleModule : Module
    {
        private readonly IModuleSettingsService<ReactionRoleSettings> _moduleSettings;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReactionRoleModule(IDiscordSettings discordSettings,
            ILogger<IModule> logger,
            IConfiguration config,
            IModuleSettingsService<ReactionRoleSettings> modulesettings,
            IServiceScopeFactory scopeFactory)
            : base(discordSettings, logger, config)
        {
            _moduleSettings = modulesettings;
            _scopeFactory = scopeFactory;
        }

        public override Task Init(DiscordSocketClient client)
        {
            client.ReactionAdded += OnReactionAdded;
            client.ReactionRemoved += OnReactionRemoved;

            _moduleSettings.SettingsChanged += (s, e) =>
            {
                var gId = e.Settings.GuildId;
            };

            return Task.CompletedTask;
        }

        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var moduleSettings = await _moduleSettings.GetSettingsByGuild(guildId);
            return moduleSettings?.Enabled ?? false;
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage,
            ISocketMessageChannel channel,
            SocketReaction socketReaction)
        {
            await SetRole(channel,
                socketReaction,
                async (role, user, userHasRole) =>
                {
                    if (userHasRole)
                        return;

                    await user.AddRoleAsync(role, new RequestOptions
                    {
                        AuditLogReason = "",
                        RetryMode = RetryMode.RetryRatelimit
                    });
                });
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage,
            ISocketMessageChannel channel,
            SocketReaction socketReaction)
        {
            await SetRole(channel,
                socketReaction,
                async (role, user, userHasRole) =>
                {
                    if (!userHasRole)
                        return;

                    await user.RemoveRoleAsync(role, new RequestOptions
                    {
                        AuditLogReason = "",
                        RetryMode = RetryMode.RetryRatelimit
                    });
                });
        }

        private async Task SetRole(ISocketMessageChannel channel,
            SocketReaction socketReaction,
            Func<IRole, SocketGuildUser, bool, Task> roleSelector)
        {
            if (channel is SocketTextChannel textChannel
                && socketReaction.Emote is Emote gEmote
                && socketReaction.User.IsSpecified
                && socketReaction.User.Value is SocketGuildUser guildUser)
            {
                var settings = await _moduleSettings.GetSettingsByGuild(textChannel.Guild.Id);

                if (!settings?.Enabled ?? false)
                    return;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var messageService = scope.ServiceProvider.GetService<IEntityService<ReactionRoleMessage>>();

                    var reactionMessages = await messageService
                        .Get(msg => msg.GuildId == textChannel.Guild.Id
                            && msg.ChannelId == textChannel.Id
                            && msg.MessageId == socketReaction.MessageId,
                            msg => msg.RollMappings);

                    var reactionMessage = reactionMessages.FirstOrDefault();

                    if (reactionMessage is null)
                        return;

                    var messageMapping = reactionMessage.RollMappings
                        .FirstOrDefault(map => map.EmoteId == gEmote.Id);

                    if (messageMapping is null)
                        return;
                    
                    var roleToAdd = textChannel.Guild.GetRole(messageMapping.RoleId);
                    await roleSelector(roleToAdd, guildUser, guildUser.Roles.Any(role => role.Id == messageMapping.RoleId));
                }
            }
        }
    }
}
