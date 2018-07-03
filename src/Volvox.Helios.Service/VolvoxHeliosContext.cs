using Microsoft.EntityFrameworkCore;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Service
{
    public class VolvoxHeliosContext : DbContext
    {
        public VolvoxHeliosContext(DbContextOptions options)
            : base(options)
        { }
        
        public DbSet<NowStreamingSettings> NowStreamingSettings { get; set; }
    }
}