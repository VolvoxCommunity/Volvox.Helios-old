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

        public override Task Init(DiscordSocketClient client)
        {
            client.MessageReceived += MessageRecieved;

            client.MessageDeleted += MessageDeleted;

            return Task.CompletedTask;
        }

        private async Task MessageRecieved(SocketMessage message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<Message>>();

                await messageService.Create(new Message
                {
                    Id = message.Id,
                    AuthorId = message.Author.Id,
                    ChannelId = message.Channel.Id
                });
            }
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<Message>>();

                var m = await messageService.Find(message.Id);

                if (m != null) await messageService.Remove(m);
            }
        }
    }
}