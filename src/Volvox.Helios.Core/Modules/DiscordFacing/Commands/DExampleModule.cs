using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Volvox.Helios.Core.Modules.DiscordFacing.Commands
{
    public class DExampleModule : IDiscordFacingModule
    {
        public string Trigger { get; private set; }

        public Task Initialize()
        {
            Trigger = "help";
            return Task.CompletedTask;
        }
        
        public async Task ExecuteAsync(DiscordFacingContext context)
        {
            await context.Channel.SendMessageAsync("no");
        }
    }
}