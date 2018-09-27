using Microsoft.EntityFrameworkCore;
using Volvox.Helios.Core.Modules.StreamAnnouncer;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.Module.ChatTracker;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Service
{
    public class VolvoxHeliosContext : DbContext
    {
        public VolvoxHeliosContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<StreamAnnouncerSettings> StreamAnnouncerSettings { get; set; }

        public DbSet<StreamAnnouncerChannelSettings> StreamAnnouncerChannelSettings { get; set; }

        public DbSet<StreamerRoleSettings> StreamerRoleSettings { get; set; }

        public DbSet<StreamAnnouncerMessage> StreamAnnouncerMessages { get; set;  }

        #region ChatTracker

        public DbSet<ChatTrackerSettings> ChatTrackerSettings { get; set; }

        public DbSet<Message> Messages { get; set; }

        #endregion
    }
}