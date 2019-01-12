using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService
{
    public class UserWarningsService : IUserWarningsService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UserWarningsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <inheritdoc />
        public async Task<UserWarnings> GetUser(ulong userId, ulong guildId, params Expression<Func<UserWarnings, object>>[] includes)
        {
            UserWarnings userData;

            using (var scope = _scopeFactory.CreateScope())
            {
                var userWarningService = scope.ServiceProvider.GetRequiredService<IEntityService<UserWarnings>>();

                var userDataDb = await userWarningService.GetFirst(u => u.UserId == userId, includes);

                // User isn't tracked yet, so create new entry for them.
                if (userDataDb == null)
                {
                    userData = new UserWarnings
                    {
                        GuildId = guildId,
                        UserId = userId
                    };

                    await userWarningService.Create(userData);
                }
                else
                {
                    userData = userDataDb;
                }
            }

            return userData;
        }
    }
}
