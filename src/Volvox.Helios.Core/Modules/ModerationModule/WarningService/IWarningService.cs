using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.WarningService
{
    public interface IWarningService
    {
        Task AddWarning(ModerationSettings moderationSettings, SocketGuildUser user, UserWarnings userData, WarningType warningType);
    }
}
