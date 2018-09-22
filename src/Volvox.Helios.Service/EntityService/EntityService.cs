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
    public class EntityService<T> : EntityServiceBase<T>
        where T : class
    {
        /// <summary>
        ///     Initialize a new EntityService class.
        /// </summary>
        /// <param name="context">Volvox.Helios context.</param>
        public EntityService(VolvoxHeliosContext context)
            : base(context) { }
    }
}