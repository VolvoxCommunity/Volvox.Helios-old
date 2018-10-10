using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Volvox.Helios.Core.Modules.StreamAnnouncer;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.Module.ChatTracker;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
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
        
        public DbSet<PollSettings> PollSettings { get; set; }
        
        #region ChatTracker

        public DbSet<ChatTrackerSettings> ChatTrackerSettings { get; set; }

        public DbSet<Message> Messages { get; set; }

        #endregion

        #region Reminder
        public DbSet<RemembotSettings> ReminderSettings { get; set; }
        public DbSet<RecurringReminderMessage> RecurringReminderMessages { get; set; }
        #endregion

        #region ModerationSettings
        public DbSet<ModerationSettings> ModerationSettings { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SetupForReminderSchema(modelBuilder);            
        }

        private void SetupForReminderSchema(ModelBuilder modelBuilder)
        {
            var reminderModel = modelBuilder.Entity<RemembotSettings>();
            var recurringReminderModel = modelBuilder.Entity<RecurringReminderMessage>();

            reminderModel.HasMany(x => x.RecurringReminders);

            recurringReminderModel.HasKey(x => x.Id)
                .ForSqlServerIsClustered();

            recurringReminderModel.Property(x => x.Id)
                .UseSqlServerIdentityColumn();

            recurringReminderModel
                .HasIndex(x => x.GuildId);

            recurringReminderModel.Property(x => x.Enabled)
                .IsRequired();

            recurringReminderModel.Property(x => x.ChannelId)
                .IsRequired();

            recurringReminderModel.Property(x => x.Message)
                .IsRequired();

            recurringReminderModel.Property(x => x.CronExpression)
                .HasMaxLength(255)
                .IsRequired();

            recurringReminderModel.Property(x => x.Fault)
                .HasDefaultValue(RecurringReminderMessage.FaultType.None)
                .IsRequired();
        }
    }
}