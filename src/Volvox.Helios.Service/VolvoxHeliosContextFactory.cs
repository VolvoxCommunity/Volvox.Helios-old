using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Volvox.Helios.Service
{
    public class VolvoxHeliosContextFactory : IDesignTimeDbContextFactory<VolvoxHeliosContext>
    {
        public VolvoxHeliosContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<VolvoxHeliosContext>();
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DEV_CONNECTION_STRING") ?? throw new ArgumentException("Connection string not found!"));

            return new VolvoxHeliosContext(optionsBuilder.Options);
        }
    }
}