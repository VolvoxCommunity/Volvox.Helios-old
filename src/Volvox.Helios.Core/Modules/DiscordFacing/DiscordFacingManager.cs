using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class DiscordFacingManager : IModule
    {
        private readonly IList<TriggerableModuleDecorator> modules;

        public DiscordFacingManager(IList<TriggerableModuleDecorator> modules)
        {
            this.modules = modules;
        }

        public void Enable()
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public void Disable()
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public async Task InvokeAsync(DiscordFacingContext discordFacingContext)
        {
            if (!(discordFacingContext.Channel is IDMChannel || discordFacingContext.Message.Author.IsBot)) return;

            foreach (var module in modules)
            {
                await module.InvokeAsync(discordFacingContext).ConfigureAwait(false);
            }
        }
    }
}
