using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.Command.Framework;

namespace Volvox.Helios.Core.Modules.Command.Commands
{
    public class HelpCommand : ICommand
    {
        public async Task TryTrigger(CommandContext context)
        {
            if (!context.Message.Content.StartsWith($"{context.GivenPrefix}help"))
            {
                throw new TriggerFailException();
            }

            await Execute(context);
        }

        public async Task Execute(CommandContext context)
        {
            await context.Channel.SendMessageAsync("Visit http://www.volvox.tech to manage the bot.");
        }
    }
}