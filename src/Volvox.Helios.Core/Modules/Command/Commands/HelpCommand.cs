using System.Threading.Tasks;
using Discord;
using Volvox.Helios.Core.Modules.Command.Framework;
using Volvox.Helios.Core.Utilities;

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
            var embed = new EmbedBuilder()
                .WithColor(EmbedColors.LogoColor)
                .WithDescription("All of the management for this bot can be found [here!](https://www.volvox.tech)");
            await context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}