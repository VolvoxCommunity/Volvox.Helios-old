using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentCache;
using Microsoft.EntityFrameworkCore;

namespace Volvox.Helios.Service.EntityService
{
    /// <summary>
    /// Caching version of the standard <see cref="EntityService{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachedEntityService<T> : EntityServiceBase<T>
        where T : class
    {
        private readonly ICache _cache;

        public CachedEntityService(VolvoxHeliosContext context, ICache cache)
            : base(context)
        {
            _cache = cache;
        }

        /// <summary>
        ///     Get the first entity from the cache or database that matches the primary key. 
        /// </summary>
        /// <param name="keys">Primary keys used to find the entity.</param>
        /// <returns>First entity matching the primary key.</returns>
        public override Task<T> Find(params object[] keys)
        {
            return _cache.WithKey(GetCacheKey(keys))
                .RetrieveUsingAsync(() => base.Find(keys))
                .ExpireAfter(TimeSpan.FromDays(1))
                .GetValueAsync();
        }

        /// <summary>
        ///     Update an entity in database and clear the value if this entity is cached.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        public async override Task Update(T entity)
        {
            await base.Create(entity);
            InvalidateFor(entity);
        }

        /// <summary>
        ///     Remove an entity from the database and clear the value if this entity is cached.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public async override Task Remove(T entity)
        {
            await base.Remove(entity);
            InvalidateFor(entity);
        }

        /// <summary>
        ///     Remove entities from the database and clear the values of already cached entities.
        /// </summary>
        /// <param name="entities">Entities to remove.</param>
        public async override Task RemoveBulk(IEnumerable<T> entities)
        {
            await base.RemoveBulk(entities);
            InvalidateForBulk(entities);
        }

        /// <summary>
        ///     Invalidate this entity by removing its value from the cache.
        /// </summary>
        /// <param name="entity">Entity to invalidate.</param>
        private void InvalidateFor(T entity)
        {
            var primaryKeys = GetPrimaryKeyValues(entity);
            var cacheKey = GetCacheKey(primaryKeys);
            _cache.WithKey(cacheKey);
        }

        /// <summary>
        ///     Invalidate entries by removing it's value from the cache.
        /// </summary>
        /// <param name="entities">Entities to invalidate.</param>
        private void InvalidateForBulk(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
                InvalidateFor(entity);           
        }

        /// <summary>
        ///     Retrieves the primary key values from the entity by using the Entity Framework metadata.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Throws when no primary key is defined on this entity type.</exception>
        private object[] GetPrimaryKeyValues(T entity)
        {
            var pks = Context.Entry(entity)
                .Properties
                .Where(x => x.Metadata.IsPrimaryKey());

            if (pks is null || !pks.Any())
                throw new InvalidOperationException($"Please specify a primary key for {typeof(T).Name}.");

            return pks.Select(x => x.CurrentValue)
                .ToArray();
        }

        /// <summary>
        ///     Generates a cache key by using the primary key values of the entity.
        /// </summary>
        /// <param name="pks"></param>
        /// <returns></returns>
        private static string GetCacheKey(object[] pks)
        {
            if (pks.Length == 1)
                return $"Entity:{typeof(T)},PrimaryKey:{pks[0]}";

            var sb = new StringBuilder($"Entity:{typeof(T)}");

            for (var i = 0; i < pks.Length; i++)
                sb.Append($",PrimaryKey_{i}:{pks[i]}");

            return sb.ToString();
        }
    }
}
