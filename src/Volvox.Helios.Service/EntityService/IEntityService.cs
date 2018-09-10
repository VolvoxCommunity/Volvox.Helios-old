using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Common;

namespace Volvox.Helios.Service.EntityService
{
    public interface IEntityService<T> where T : Entity
    {
        /// <summary>
        ///     Save an entity to the database.
        /// </summary>
        /// <param name="entity">Entity to save to the database.</param>
        Task Save(T entity);

        /// <summary>
        ///     Get an entity from the database.
        /// </summary>
        /// <param name="id">Id of the entity.</param>
        /// <param name="includes">Properties to eagerly load.</param>
        /// <returns>Entity with the matching id.</returns>
        Task<T> Get(int id, params Expression<Func<T, object>>[] includes);

        /// <summary>
        ///     Remove an entity from the database.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        Task Remove(T entity);
    }
}