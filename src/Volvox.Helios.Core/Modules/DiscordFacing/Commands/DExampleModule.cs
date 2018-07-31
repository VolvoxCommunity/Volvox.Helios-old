using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Core.Modules.Common;

namespace Volvox.Helios.Core.Modules.DiscordFacing.Commands
{
    public class DExampleModule : IDiscordFacingModule, ITrigger
    {
        public async Task ExecuteAsync(DiscordFacingContext context)
        {
            await context.Channel.SendMessageAsync("no");
        }

        public async Task<bool> Trigger(DiscordFacingContext context)
        {
            if (context.Message.Content.StartsWith($"{context.GivenPrefix}help"))
            {
                await ExecuteAsync(context);
                return true;
            }

            return false;
        }
    }
}