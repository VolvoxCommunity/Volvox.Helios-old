using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Service.EntityService
{
    public class EntityChangedDispatcher<T>
    {
        public event EventHandler<EntityChangedEventArgs<T>> EntityCreated;
        public event EventHandler<EntityChangedEventArgs<T>> EntityUpdated;
        public event EventHandler<EntityChangedEventArgs<T>> EntityDeleted;

        internal void OnEntityCreated(object sender, T entity)
        {
            EntityCreated?.Invoke(sender, new EntityChangedEventArgs<T>(entity));
        }

        internal void OnEntityUpdated(object sender, T entity)
        {
            EntityUpdated?.Invoke(sender, new EntityChangedEventArgs<T>(entity));
        }

        internal void OnEntityDeleted(object sender, T entity)
        {
            EntityDeleted?.Invoke(sender, new EntityChangedEventArgs<T>(entity));
        }
    }
}
