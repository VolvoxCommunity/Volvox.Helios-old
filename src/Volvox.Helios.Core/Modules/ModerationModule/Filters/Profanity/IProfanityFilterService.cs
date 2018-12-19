using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters.Profanity
{
    public interface IProfanityFilterService
    {
        bool ProfanityCheck(SocketMessage message, ProfanityFilter profanityFilter);
    }
}
