using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.Common;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class MessageReceiverModuleDecorator : IModule
    {
        private readonly DiscordSocketClient discordSocketClient;
        private readonly IModule module;

        public MessageReceiverModuleDecorator(DiscordSocketClient discordSocketClient, IModule module)
        {
            this.discordSocketClient = discordSocketClient;
            this.module = module;
        }

        public void Enable()
        {
            this.discordSocketClient.MessageReceived += DiscordSocketClientOnMessageReceived;
        }

        private async Task DiscordSocketClientOnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is SocketUserMessage socketUserMessage)
                await this.InvokeAsync(new DiscordFacingContext(socketUserMessage, discordSocketClient)).ConfigureAwait(false);
        }

        public void Disable()
        {
            this.discordSocketClient.MessageReceived -= DiscordSocketClientOnMessageReceived;
        }


        public async Task InvokeAsync(DiscordFacingContext discordFacingContext)
        {
            await this.module.InvokeAsync(discordFacingContext).ConfigureAwait(false);
        }
    }
}
