using System;
using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.Common;

namespace Volvox.Helios.Core.Modules.DiscordFacing.Commands
{
    public class HelpModule : IModule
    {
        public void Enable()
        {
            // TODO Can you even enable help?
            throw new NotImplementedException();
        }

        public void Disable()
        {
            // TODO Can you even disable help?
            throw new NotImplementedException();
        }

        public async Task InvokeAsync(DiscordFacingContext discordFacingContext)
        {
            // TODO
            await discordFacingContext.Channel.SendMessageAsync("TODO: Provide Help Message").ConfigureAwait(false);
        }
    }
}
