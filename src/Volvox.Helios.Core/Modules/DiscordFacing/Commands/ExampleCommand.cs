using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.DiscordFacing.Framework;

namespace Volvox.Helios.Core.Modules.DiscordFacing.Commands
{
    public class ExampleCommand : ICommand
    {
        public async Task TryTrigger(DiscordFacingContext context)
        {
            if (context.Message.Content.StartsWith($"{context.GivenPrefix}helloworld") == false)
            {
                throw new TriggerFailException();
            }
            await Execute(context);
        }

        public async Task Execute(DiscordFacingContext context)
        {
            await context.Channel.SendMessageAsync("the world doesn't want you here");
        }
    }
}