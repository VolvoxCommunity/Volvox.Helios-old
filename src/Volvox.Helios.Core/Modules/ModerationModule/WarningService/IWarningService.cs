using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.WarningService
{
    public interface IWarningService
    {
        Task<Warning> AddWarning(SocketGuildUser user, FilterType warningType);

        Task RemoveWarning(Warning warning);

        Task RemoveWarningBulk(List<Warning> warnings);
    }
}
