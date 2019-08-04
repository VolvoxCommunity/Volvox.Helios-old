using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Volvox.Helios.Service;

namespace Volvox.Helios.Web
{
    public class VolvoxHeliosContextFactory : IDesignTimeDbContextFactory<VolvoxHeliosContext>
    {
        public VolvoxHeliosContext CreateDbContext(string[] args)
        {
            var configuration = ConfigurationHelper.GetDefaultConfiguration();

            var optionsBuilder = new DbContextOptionsBuilder<VolvoxHeliosContext>();

            optionsBuilder.UseSqlServer(configuration.GetConnectionString("VolvoxHeliosDatabase"), options=>
            options.MigrationsAssembly("Volvox.Helios.Web"));
            return new VolvoxHeliosContext(optionsBuilder.Options);
        }
    }
}