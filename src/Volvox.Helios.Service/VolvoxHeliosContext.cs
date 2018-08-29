using Microsoft.EntityFrameworkCore;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Service
{
    public class VolvoxHeliosContext : DbContext
    {
        public VolvoxHeliosContext(DbContextOptions options)
            : base(options)
        { }
        
        public DbSet<StreamAnnouncerSettings> StreamAnnouncerSettings { get; set; }

        public DbSet<StreamerRoleSettings> StreamerRoleSettings { get; set; }
    }
}