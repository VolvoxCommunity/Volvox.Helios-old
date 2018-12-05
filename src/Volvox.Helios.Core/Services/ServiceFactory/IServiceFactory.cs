using System;
using System.Collections.Generic;
using System.Text;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Services.MessageService;

namespace Volvox.Helios.Core.Services.ServiceFactory
{
    /// <summary>
    ///     Get service by it's type. This is used when needing a service that would cause a dependency loop when injecting it directly.
    /// </summary>
    public interface IServiceFactory
    {
        T GetService<T>();
    }
}
