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

            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=VolvoxHelios;Trusted_Connection=True;";

            var server = Environment.GetEnvironmentVariable("DEV_DB_SERVER");
            var userName = Environment.GetEnvironmentVariable("DEV_DB_USERNAME");
            var password = Environment.GetEnvironmentVariable("DEV_DB_PASSWORD");

            if (server != null && userName != null && password != null)
                connectionString =
                    $"Server={server},1433;Initial Catalog=Volvox.Helios;Persist Security Info=False;User ID={userName};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=true;Connection Timeout=30;";

            optionsBuilder.UseSqlServer(connectionString);

            return new VolvoxHeliosContext(optionsBuilder.Options);
        }
    }
}