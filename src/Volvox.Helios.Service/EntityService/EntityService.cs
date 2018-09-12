using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentCache;
using Microsoft.EntityFrameworkCore;

namespace Volvox.Helios.Service.EntityService
{
    /// <inheritdoc />
    public class EntityService<T> : IEntityService<T> where T : class
    {
        private readonly VolvoxHeliosContext _context;
        private readonly ICache _cache;

        /// <summary>
        ///     Initialize a new EntityService class.
        /// </summary>
        /// <param name="context">Volvox.Helios context.</param>
        public EntityService(VolvoxHeliosContext context, ICache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <inheritdoc />
        public Task<T> Find(params object[] keys)
        {
            return _cache.WithKey(GetCacheKey(keys))
                .RetrieveUsingAsync(() => _context.Set<T>().FindAsync(keys))
                .ExpireAfter(TimeSpan.FromDays(1))
                .GetValueAsync();
        }

        /// <inheritdoc />
        public Task<List<T>> Get(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes)
        {
            var query = GetIncludesQuery(includes);

            return query.Where(filter).ToListAsync();
        }

        /// <inheritdoc />
        public Task<List<T>> GetAll(params Expression<Func<T, object>>[] includes)
        {
            var query = GetIncludesQuery(includes);

            return query.ToListAsync();
        }

        /// <inheritdoc />
        public Task Create(T entity)
        {
            _context.Set<T>().Add(entity);

            return _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task Update(T entity)
        {
            if (!_context.Set<T>().Local.Any(e => e == entity))
            {
                throw new InvalidOperationException("You must use an attached entity when updating.");
            }

            await _context.SaveChangesAsync();

            var pkValues = GetPrimaryKeyValues(entity);
            _cache.WithKey(GetCacheKey(pkValues)).ClearValue();
        }

        /// <inheritdoc />
        public async Task Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();

            var pkValues = GetPrimaryKeyValues(entity);
            _cache.WithKey(GetCacheKey(pkValues)).ClearValue();
        }

        /// <summary>
        ///     Get the database query with added includes.
        /// </summary>
        /// <param name="includes">Properties to eagerly load.</param>
        /// <returns>Query set to the type and includes added.</returns>
        private IQueryable<T> GetIncludesQuery(Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            return query;
        }

        private object[] GetPrimaryKeyValues(T entity)
        {
            var pks = _context.Entry(entity)
                .Properties
                .Where(x => x.Metadata.IsPrimaryKey());

            if (pks is null || !pks.Any())
                throw new InvalidOperationException($"Please specify a primary key for {typeof(T).Name}.");

            return pks.Select(x => x.CurrentValue)
                .ToArray();
        }

        private static string GetCacheKey(object[] pks)
        {
            if(pks.Length == 1)
                return $"Entity:{typeof(T)},PrimaryKey:{pks[0]}";

            var sb = new StringBuilder($"Entity:{typeof(T)}");

            for (var i = 0; i < pks.Length; i++)
                sb.Append($",PrimaryKey_{i}:{pks[i]}");

            return sb.ToString();
        }
    }
}