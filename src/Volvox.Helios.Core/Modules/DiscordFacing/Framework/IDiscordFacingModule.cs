using System.Threading.Tasks;

namespace Volvox.Helios.Core.Modules.DiscordFacing.Framework
{
    public interface IDiscordFacingModule
    {
        Task TryTrigger(DiscordFacingContext context);

        Task Execute(DiscordFacingContext context);
    }
}