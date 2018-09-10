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
        ///     Get the first entity from the database that matches the primary key.
        /// </summary>
        /// <param name="key">Primary key used to find the entity.</param>
        /// <returns>First entity matching the primary key.</returns>
        Task<T> Find(object key);

        /// <summary>
        ///     Get all entities that match the filter.
        /// </summary>
        /// <param name="filter">Filter to use to get the entities.</param>
        /// <param name="includes">Properties to eagerly load.</param>
        /// <returns>List of entities that match the filer.</returns>
        Task<List<T>> Get(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes);

        /// <summary>
        ///     Get all entities from the database.
        /// </summary>
        /// <param name="includes">Properties to eagerly load.</param>
        /// <returns>List of all entities from the database.</returns>
        Task<List<T>> GetAll(params Expression<Func<T, object>>[] includes);

        /// <summary>
        ///     Save an entity to the database.
        /// </summary>
        /// <param name="entity">Entity to save to the database.</param>
        Task Save(T entity);

        /// <summary>
        ///     Remove an entity from the database.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        Task Remove(T entity);
    }
}