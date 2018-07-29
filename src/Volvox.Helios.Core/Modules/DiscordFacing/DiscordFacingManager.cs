using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class DiscordFacingManager : Module, IModule
    {
        public IList<IDiscordFacingModule> Modules { get; private set; }
        private DiscordSocketClient Client { get; set; }
             
        public DiscordFacingManager(IDiscordSettings discordSettings, ILogger<IModule> logger, IList<IDiscordFacingModule> modules) : base(discordSettings, logger)
        {
            Modules = modules;
        }
        
        public override Task Init(DiscordSocketClient client)
        {
            Client = client;
            Client.MessageReceived += HandleCommandAsync;
            foreach (var mod in Modules)
            {
                Logger.LogInformation($"DiscordFacing: Initializing {mod.GetType().Name}");
                mod.Initialize();
            }
            return Task.CompletedTask;
        }

        public async Task HandleCommandAsync(SocketMessage m)
        {
            // h- is the placeholder prefix while Bapes finishes the settings framework
            if (!(m is SocketUserMessage message) || message.Channel is IDMChannel || message.Author.IsBot) return;
            var context = new DiscordFacingContext(message, Client);
            foreach (var module in Modules)
            {
                if (message.Content.StartsWith($"h-{module.Trigger}"))
                    await module.ExecuteAsync(context);


            }
        }


        public override Task Execute(DiscordSocketClient client) => Task.CompletedTask;
    }
}