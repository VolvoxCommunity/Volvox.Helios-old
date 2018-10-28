using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module.ChatTracker;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Modules.ChatTracker
{
    public class ChatTrackerModule : Module
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatTrackerModule(IDiscordSettings discordSettings, ILogger<IModule> logger, IConfiguration config,
            IServiceScopeFactory scopeFactory) : base(discordSettings, logger, config)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        ///     Initialize the module by subscribing to the events.
        /// </summary>
        /// <param name="client">Client to subscribe to.</param>
        public override Task Init(DiscordSocketClient client)
        {
            client.MessageReceived += MessageReceived;

            client.MessageDeleted += MessageDeleted;

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Add the message to the database.
        /// </summary>
        /// <param name="message">Message to add.</param>
        private async Task MessageReceived(SocketMessage message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<Message>>();

                if (message.Channel is IGuildChannel guildChannel && !message.Author.IsBot)
                    await messageService.Create(new Message
                    {
                        Id = message.Id,
                        AuthorId = message.Author.Id,
                        GuildId = guildChannel.GuildId,
                        ChannelId = guildChannel.Id,
                        Timestamp = message.Timestamp
                    });
            }
        }

        /// <summary>
        ///     Mark the message as deleted.
        /// </summary>
        /// <param name="message">Message to delete.</param>
        private async Task MessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<Message>>();

                var m = await messageService.Find(message.Id);

                if (m != null)
                {
                    m.Deleted = true;

                    await messageService.Update(m);
                }
            }
        }
    }
}