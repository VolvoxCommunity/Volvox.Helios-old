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
    public abstract class EntityServiceBase<T> : IEntityService<T>
        where T : class
    {
        protected readonly VolvoxHeliosContext Context;

        /// <summary>
        ///     Initialize a new EntityService class.
        /// </summary>
        /// <param name="context">Volvox.Helios context.</param>
        protected EntityServiceBase(VolvoxHeliosContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public virtual Task<T> Find(params object[] keys)
        {
            return Context.FindAsync<T>(keys);
        }

        /// <inheritdoc />
        public virtual Task<List<T>> Get(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes)
        {
            var query = GetIncludesQuery(includes);
            return query.Where(filter).ToListAsync();
        }

        /// <inheritdoc />
        public virtual Task<List<T>> GetAll(params Expression<Func<T, object>>[] includes)
        {
            var query = GetIncludesQuery(includes);
            return query.ToListAsync();
        }

        /// <inheritdoc />
        public virtual Task Create(T entity)
        {
            Context.Set<T>().Add(entity);
            return Context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task Update(T entity)
        {
            if (!Context.Set<T>().Local.Any(e => e == entity))
            {
                throw new InvalidOperationException("You must use an attached entity when updating.");
            }

            await Context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task Remove(T entity)
        {
            Context.Set<T>().Remove(entity);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        ///     Get the database query with added includes.
        /// </summary>
        /// <param name="includes">Properties to eagerly load.</param>
        /// <returns>Query set to the type and includes added.</returns>
        protected IQueryable<T> GetIncludesQuery(Expression<Func<T, object>>[] includes)
        {
            var query = Context.Set<T>().AsQueryable();

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            return query;
        }
    }
}
