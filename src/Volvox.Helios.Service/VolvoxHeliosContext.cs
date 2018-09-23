using Microsoft.EntityFrameworkCore;
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

        #region ChatTracker

        public DbSet<ChatTrackerSettings> ChatTrackerSettings { get; set; }

        public DbSet<Message> Messages { get; set; }

        #endregion

        #region Reminder
        public DbSet<ReminderSettings> ReminderSettings { get; set; }
        public DbSet<RecurringReminderMessage> RecurringReminderMessages { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SetupForReminderSchema(modelBuilder);            
        }

        private void SetupForReminderSchema(ModelBuilder modelBuilder)
        {
            var reminderModel = modelBuilder.Entity<ReminderSettings>();
            var recurringReminderModel = modelBuilder.Entity<RecurringReminderMessage>();

            reminderModel.HasMany(x => x.RecurringReminders);

            recurringReminderModel.HasKey(x => x.Id)
                .ForSqlServerIsClustered();

            recurringReminderModel.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            recurringReminderModel
                .HasIndex(x => x.GuildId);

            recurringReminderModel.Property(x => x.Enabled)
                .IsRequired();

            recurringReminderModel.Property(x => x.ChannelId)
                .IsRequired();

            recurringReminderModel.Property(x => x.Message)
                .IsRequired();

            recurringReminderModel.Property(x => x.ReadableCronExpression)
                .IsRequired();

            recurringReminderModel.Property(x => x.JobId)
                .IsRequired();

            recurringReminderModel.Property(x => x.CronExpression)
                .IsRequired();
        }
    }
}