using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Volvox.Helios.Service.EntityService
{
    /// <summary>
    ///     Entity service to manage CRUD operations to the database.
    /// </summary>
    public interface IEntityService<T> where T : class
    {
        /// <summary>
        ///     Save an entity to the database.
        /// </summary>
        /// <param name="entity">Entity to save to the database.</param>
        Task Save(T entity);

        /// <summary>
        ///     Get the first entity from the database that matches the filter.
        /// </summary>
        /// <param name="filter">Filter to use to get the entity.</param>
        /// <param name="includes">Properties to eagerly load.</param>
        /// <returns>Entity with the matching id.</returns>
        Task<T> Get(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes);

        /// <summary>
        ///     Get all entities from the database.
        /// </summary>
        /// <param name="includes">Properties to eagerly load.</param>
        /// <returns>List of all entities from the database.</returns>
        Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes);

        /// <summary>
        ///     Remove an entity from the database.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        Task Remove(T entity);
    }
}