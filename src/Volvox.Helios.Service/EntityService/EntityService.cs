using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Volvox.Helios.Service.EntityService
{
    /// <inheritdoc />
    public class EntityService<T> : IEntityService<T> where T : class
    {
        private readonly VolvoxHeliosContext _context;

        /// <summary>
        ///     Initialize a new EntityService class.
        /// </summary>
        /// <param name="context">Volvox.Helios context.</param>
        public EntityService(VolvoxHeliosContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public Task<T> Find(object key)
        {
            return _context.Set<T>().FindAsync(key);
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
        public Task Save(T entity)
        {
            _context.Set<T>().Add(entity);

            return _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task Remove(T entity)
        {
            _context.Set<T>().Remove(entity);

            return _context.SaveChangesAsync();
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
    }
}