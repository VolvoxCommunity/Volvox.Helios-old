using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Volvox.Helios.Service.ModuleSettings
{
    /// <inheritdoc />
    public class ModduleSettingsService<T> : IModuleSettingsService<T> where T : Domain.ModuleSettings.ModuleSettings
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ModduleSettingsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        
        /// <inheritdoc />
        public async Task SaveSettings(T settings)
        {
            // Create a new scope to get the db context.
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VolvoxHeliosContext>();

                await context.AddAsync(settings);
                await context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task<T> GetSettingsByGuild(ulong guildId)
        {
            // Create a new scope to get the db context.
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VolvoxHeliosContext>();

                return await context.Set<T>().FirstOrDefaultAsync(s => s.GuildId == guildId);
            }
        }
    }
}