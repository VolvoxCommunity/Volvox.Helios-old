using System;
using System.Collections.Generic;
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
        private readonly Dictionary<ulong, List<string>> _guildCacheKeys;
        private readonly IServiceScopeFactory _scopeFactory;

        public ModuleSettingsService(IServiceScopeFactory scopeFactory, ICache cache)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _guildCacheKeys = new Dictionary<ulong, List<string>>();
        }

        public event EventHandler<ModuleSettingsChangedArgs<T>> SettingsChanged;

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

                // Reset all of the cache keys for the specified guild.
                if (_guildCacheKeys.ContainsKey(settings.GuildId))
                    foreach (var cacheKey in _guildCacheKeys[settings.GuildId])
                        _cache.WithKey(cacheKey).ClearValue();

                OnSettingsChanged(settings);
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
                var cacheKey = GetCacheKey(guildId, includes);

                var cachedSetting = await _cache.WithKey(cacheKey)
                    .RetrieveUsingAsync(async () =>
                    {
                        var query = context.Set<T>().AsQueryable();

                        if (includes != null)
                            query = includes.Aggregate(query, (current, include) => current.Include(include));

                        return await query.FirstOrDefaultAsync(s => s.GuildId == guildId);
                    })
                    .InvalidateIf(cachedValue => cachedValue.Value != null)
                    .ExpireAfter(TimeSpan.FromDays(1))
                    .GetValueAsync();

                // Initialize new guild cache.
                if (!_guildCacheKeys.ContainsKey(guildId))
                    _guildCacheKeys.Add(guildId, new List<string>());

                // Add cache key to the list.
                if (!_guildCacheKeys[guildId].Contains(cacheKey))
                    _guildCacheKeys[guildId].Add(cacheKey);

                return cachedSetting;
            }
        }

        /// <inheritdoc />
        public void ClearCacheByGuild(ulong guildId)
        {
            _cache.WithKey(GetCacheKey(guildId)).ClearValue();
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

                OnSettingsChanged(settings);
            }
        }

        /// <summary>
        ///     Create a unique caching key based on the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <param name="includes">Navigation property includes to eager load.</param>
        /// <returns>Cache key based on the specified guild.</returns>
        private static string GetCacheKey(ulong guildId, params Expression<Func<T, object>>[] includes)
        {
            var includesKey = "";

            // Append all of the includes to the cache key.
            if (includes.Length > 0)
                includesKey = includes.Aggregate(includesKey, (current, include) => current + include.Body);

            return $"Setting:{typeof(T).Name}Guild:{guildId}Includes:{includesKey}";
        }

        private void OnSettingsChanged(T settings)
        {
            SettingsChanged?.Invoke(this, new ModuleSettingsChangedArgs<T>(settings));
        }
    }
}