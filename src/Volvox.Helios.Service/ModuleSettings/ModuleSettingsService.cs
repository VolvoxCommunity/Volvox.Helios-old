using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Volvox.Helios.Service.ModuleSettings
{
    /// <inheritdoc />
    public class ModuleSettingsService<T> : IModuleSettingsService<T> where T : Domain.ModuleSettings.ModuleSettings
    {
        private readonly ICache _cache;
        private readonly IServiceScopeFactory _scopeFactory;

        public ModuleSettingsService(IServiceScopeFactory scopeFactory, ICache cache)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
        }

        /// <inheritdoc />
        public async Task SaveSettings(T settings)
        {
            // Create a new scope to get the db context.
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VolvoxHeliosContext>();
                var guildSetting = await context.Set<T>().FirstOrDefaultAsync(s => s.GuildId == settings.GuildId);

                // Replace the setting if it already exists.
                if (guildSetting != null)
                    context.Entry(guildSetting).CurrentValues.SetValues(settings);

                // Add the setting.
                else
                    await context.AddAsync(settings);

                await context.SaveChangesAsync();

                // Reset the cache value.
                _cache.WithKey(GetCacheKey(settings.GuildId)).ClearValue();
            }
        }

        /// <inheritdoc />
        public async Task<T> GetSettingsByGuild(ulong guildId, params Expression<Func<T, object>>[] includes)
        {
            // Create a new scope to get the db context.
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VolvoxHeliosContext>();

                // Cache the settings.
                var cachedSetting = await _cache.WithKey(GetCacheKey(guildId))
                    .RetrieveUsingAsync(async () =>
                    {
                        var query = context.Set<T>().AsQueryable();

                        if (includes != null)
                        {
                            query = includes.Aggregate(query, (current, include) => current.Include(include));
                        }

                        return await query.FirstOrDefaultAsync(s => s.GuildId == guildId);
                    })
                    .InvalidateIf(cachedValue => cachedValue.Value != null)
                    .ExpireAfter(TimeSpan.FromDays(1))
                    .GetValueAsync();

                return cachedSetting;
            }
        }

        /// <inheritdoc />
        public async Task RemoveSetting(T settings)
        {
            // Create a new scope to get the db context.
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VolvoxHeliosContext>();

                context.Remove(settings);

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        ///     Create a unique caching key based on the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>Cache key based on the specified guild.</returns>
        private static string GetCacheKey(ulong guildId)
        {
            return $"Setting:{typeof(T).Name}Guild:{guildId}";
        }
    }
}