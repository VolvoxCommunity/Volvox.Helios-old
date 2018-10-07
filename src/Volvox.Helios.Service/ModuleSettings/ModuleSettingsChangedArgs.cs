using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Service.ModuleSettings
{
    public class ModuleSettingsChangedArgs<T>
    {
        public T Settings { get; }

        internal ModuleSettingsChangedArgs(T settings)
        {
            Settings = settings;
        }
    }
}
