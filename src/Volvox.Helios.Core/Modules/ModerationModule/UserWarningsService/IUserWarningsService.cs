using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule;

namespace Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService
{
    public interface IUserWarningsService
    {
        Task<UserWarnings> GetUser(ulong userId, ulong guildId, params Expression<Func<UserWarnings, object>>[] includes);
    }
}
