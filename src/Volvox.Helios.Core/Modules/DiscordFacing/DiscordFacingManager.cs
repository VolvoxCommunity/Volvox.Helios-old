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
        public IList<IModule> Modules { get; }

        private DiscordSocketClient Client { get; set; }
             
        public DiscordFacingManager(IDiscordSettings discordSettings, ILogger<IModule> logger, IList<IModule> modules) : base(discordSettings, logger)
        {
            Modules = modules;
        }

        /// <inheritdoc />
        /// <summary>
        /// Binds DiscordFacingManager to the provided client and initializes DModule properties.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        /// <returns></returns>
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
        public async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage socketUserMessage) || socketUserMessage.Channel is IDMChannel || socketUserMessage.Author.IsBot) return;
            var context = new DiscordFacingContext(socketUserMessage, Client);

            foreach (var module in Modules)
            {
                if (await module.(context).ConfigureAwait(false)) break;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Base implementation throws an exception.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        /// <returns></returns>
        public override Task Execute(DiscordSocketClient client) => Task.CompletedTask;
    }
}