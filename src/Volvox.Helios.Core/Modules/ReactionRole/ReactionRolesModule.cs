using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
using Newtonsoft.Json.Serialization;
using Volvox.Helios.Service.Discord.Guild;

namespace Volvox.Helios.Core.Modules.ReactionRole
{
    public class ReactionRolesModule : Module
    {
        private readonly IModuleSettingsService<ReactionRoleSettings> _moduleSettings;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EntityChangedDispatcher<ReactionRolesMessage> _entityChangedDispatcher;
        private DiscordSocketClient _client;

        public ReactionRolesModule(IDiscordSettings discordSettings,
            ILogger<IModule> logger,
            IConfiguration config,
            IModuleSettingsService<ReactionRoleSettings> moduleSettings,
            IServiceScopeFactory scopeFactory,
            EntityChangedDispatcher<ReactionRolesMessage> reactionRolesDispatch)
            : base(discordSettings, logger, config)
        {
            _moduleSettings = moduleSettings;
            _scopeFactory = scopeFactory;
            _entityChangedDispatcher = reactionRolesDispatch;
        }

        public override Task Init(DiscordSocketClient client)
        {
            _client = client;
            client.ReactionAdded += OnReactionAdded;
            client.ReactionRemoved += OnReactionRemoved;

            _entityChangedDispatcher.EntityCreated += OnMessageCreated;
            _entityChangedDispatcher.EntityUpdated += OnMessageUpdated;
            _entityChangedDispatcher.EntityDeleted += OnMessageDeleted;

            return Task.CompletedTask;
        }

        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var moduleSettings = await _moduleSettings.GetSettingsByGuild(guildId);
            return moduleSettings?.Enabled ?? false;
        }

        private async void OnMessageCreated(object sender, EntityChangedEventArgs<ReactionRolesMessage> args)
        {
            var channel = _client.GetChannel(args.Entity.ChannelId);
            var msg = args.Entity;

            using (var scope = _scopeFactory.CreateScope())
            {
                var guildService = scope.ServiceProvider.GetService<IDiscordGuildService>();
                var entityService = scope.ServiceProvider.GetService<IEntityService<ReactionRolesMessage>>();
                var roles = (await guildService.GetRoles(msg.GuildId))
                    .ToDictionary(k => k.Id);

                var emotes = (await guildService.GetEmojis(msg.GuildId))
                    .ToDictionary(k => k.Id);

                if (channel is SocketTextChannel stc)
                {
                    var footer = new EmbedFooterBuilder()
                        .WithText("")
                        .Build();

                    var embed = new EmbedBuilder()
                        .WithTitle(msg.Title)
                        .WithColor(Color.Green)
                        .WithDescription(msg.Message);

                    foreach (var mapping in msg.RollMappings)
                    {
                        if (roles.TryGetValue(mapping.RoleId, out var role)
                            && emotes.TryGetValue(mapping.EmoteId, out var emote))
                        {
                            embed.AddField(role.Name, $":{emote.Name}:", true);
                        }
                    }

                    var postedMessage = await stc.SendMessageAsync(embed: embed.Build(), options: new RequestOptions
                    {
                        RetryMode = RetryMode.RetryRatelimit
                    });

                    var semotes = stc.Guild.Emotes;
                    var sroles = stc.Guild.Roles;
                    foreach (var mapping in msg.RollMappings)
                    {
                        var e = semotes.FirstOrDefault(x => x.Id == mapping.EmoteId);

                        await postedMessage.AddReactionAsync(e, new RequestOptions
                        {
                            RetryMode = RetryMode.RetryRatelimit
                        });
                    }

                    var msgToUpdate = await entityService.Find(msg.Id);
                    msgToUpdate.MessageId = postedMessage.Id;
                    await entityService.Update(msgToUpdate);
                }
            }
        }

        private async void OnMessageUpdated(object sender, EntityChangedEventArgs<ReactionRolesMessage> args)
        {

        }

        private async void OnMessageDeleted(object sender, EntityChangedEventArgs<ReactionRolesMessage> args)
        {

        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage,
            ISocketMessageChannel channel,
            SocketReaction socketReaction)
        {
            await SetRole(channel,
                socketReaction,
                async (role, user, userHasRole) =>
                {
                    try
                    {


                        if (userHasRole)
                            return;


                        await user.AddRoleAsync(role, new RequestOptions
                        {
                            AuditLogReason = "",
                            RetryMode = RetryMode.RetryRatelimit
                        });
                    }
                    catch (Exception ex)
                    {

                    }
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var messageService = scope.ServiceProvider.GetService<IEntityService<ReactionRolesMessage>>();

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
