using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class DiscordFacingManager : Module
    {
        public IList<ITriggerable> Modules { get; private set; }
        private DiscordSocketClient Client { get; set; }
             
        public DiscordFacingManager(IDiscordSettings discordSettings, ILogger<IModule> logger, IList<ITriggerable> modules) : base(discordSettings, logger)
        {
            Modules = modules;
        }
        
        /// <summary>
        /// Binds DiscordFacingManager to the provided client and initializes DModule properties.
        /// </summary>
        public override Task Init(DiscordSocketClient client)
        {
            Client = client;
            Client.MessageReceived += HandleCommandAsync;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called every time the bound client emits a SocketMessage. Used to match to a DModule and execute with a
        /// context created with the message.
        /// </summary>
        public async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage message) || message.Channel is IDMChannel || message.Author.IsBot) return;
            var context = new DiscordFacingContext(message, Client, "h-"); // h- is the placeholder prefix while Bapes finishes the settings framework

            foreach (var mod in Modules)
            {
                if (await mod.TryTrigger(context)) break;
            }
        }
        
        /// <summary>
        /// Base implementation throws an exception.
        /// </summary>
        public override Task Execute(DiscordSocketClient client) => Task.CompletedTask;
    }
}