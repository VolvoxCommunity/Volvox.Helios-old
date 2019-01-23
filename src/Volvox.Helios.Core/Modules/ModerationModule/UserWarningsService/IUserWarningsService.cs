using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule;

namespace Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService
{
    public interface IUserWarningsService
    {
        /// <summary>
        ///     Gets user data from database.
        /// </summary>
        /// <param name="userId">Discord user ID</param>
        /// <param name="guildId">Discord guild ID</param>
        /// <param name="includes">Includes for EF</param>
        /// <returns></returns>
        Task<UserWarnings> GetUser(ulong userId, ulong guildId,
            params Expression<Func<UserWarnings, object>>[] includes);
    }
}