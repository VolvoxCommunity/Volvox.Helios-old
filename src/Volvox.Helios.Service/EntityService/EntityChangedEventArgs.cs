using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Service.EntityService
{
    public class EntityChangedEventArgs<T>
    {
        public T Entity { get; }

        internal EntityChangedEventArgs(T entity)
        {
            Entity = entity;
        }
    }
}
