using System.Threading.Tasks;

namespace Volvox.Helios.Core.Modules.DiscordFacing.Framework
{
    public interface ICommand
    {
        Task TryTrigger(DiscordFacingContext context);

        Task Execute(DiscordFacingContext context);
    }
}