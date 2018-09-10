using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volvox.Helios.Domain.Common;

namespace Volvox.Helios.Service.EntityService
{
    /// <inheritdoc />
    public class EntityService<T> : IEntityService<T> where T : Entity
    {
        private readonly VolvoxHeliosContext _context;

        public EntityService(VolvoxHeliosContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task Save(T entity)
        {
            _context.Set<T>().Add(entity);

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<T> Get(int id, params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();

            if (includes != null) query = includes.Aggregate(query, (current, include) => current.Include(include));

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();

            if (includes != null) query = includes.Aggregate(query, (current, include) => current.Include(include));

            return await query.ToListAsync();
        }

        /// <inheritdoc />
        public async Task Remove(T entity)
        {
            _context.Set<T>().Remove(entity);

            await _context.SaveChangesAsync();
        }
    }
}