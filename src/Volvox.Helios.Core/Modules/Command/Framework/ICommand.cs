using System.Threading.Tasks;

namespace Volvox.Helios.Core.Modules.Command.Framework
{
    public interface ICommand
    {
        Task TryTrigger(CommandContext context);

        Task Execute(CommandContext context);
    }
}