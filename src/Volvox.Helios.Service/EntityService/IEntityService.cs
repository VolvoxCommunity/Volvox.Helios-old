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
        /// <param name="keys">Primary keys used to find the entity.</param>
        /// <returns>First entity matching the primary key.</returns>
        Task<T> Find(params object[] keys);

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
        ///     Create an entity and save to database.
        /// </summary>
        /// <param name="entity">Entity to create/save to database.</param>
        Task Create(T entity);

        /// <summary>
        ///     Create entities and save to databse.
        /// </summary>
        /// <param name="entities">Entities to create/save to database.</param>
        Task CreateBulk(IEnumerable<T> entities);

        /// <summary>
        ///     Update an entity in database.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        Task Update(T entity);

        /// <summary>
        ///     Remove an entity from the database.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        Task Remove(T entity);

        /// <summary>
        ///     Remove entities from database.
        /// </summary>
        /// <param name="entities">Entities to remove.</param>
        Task RemoveBulk(IEnumerable<T> entities);
    }
}