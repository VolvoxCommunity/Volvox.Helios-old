using Microsoft.Extensions.DependencyInjection;

namespace Volvox.Helios.Core.Services.ServiceFactory
{
    /// <inheritdoc />
    public class ServiceFactory : IServiceFactory
    {
        private IServiceScopeFactory _scopeFactory;

        public ServiceFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public T GetService<T>()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<T>();
            }
        }   
    }
}
