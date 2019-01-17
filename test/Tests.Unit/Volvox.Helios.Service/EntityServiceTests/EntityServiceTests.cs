using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentCache;
using FluentCache.Simple;
using Microsoft.EntityFrameworkCore;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.Module.ChatTracker;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service;
using Volvox.Helios.Service.EntityService;
using Xunit;

namespace Tests.Unit.Volvox.Helios.Service.EntityServiceTests
{
    public class EntityServiceTests
    {
        // Default test entity.
        private readonly Message _testMessageEntity = new Message
        {
            AuthorId = 123,
            ChannelId = 456,
            Deleted = false
        };

        /// <summary>
        ///     Create a new VolvoxHeliosContext with a unique in-memory name.
        /// </summary>
        private static VolvoxHeliosContext GetInMemoryContext()
        {
            var contextOptions = new DbContextOptionsBuilder<VolvoxHeliosContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new VolvoxHeliosContext(contextOptions);
        }

        [Fact]
        public async Task Create_OneEntity()
        {
            using (var context = GetInMemoryContext())
            {
                // Arrange
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());

                // Act
                await entityService.Create(_testMessageEntity);

                // Assert
                Assert.Single(context.Messages);
                Assert.Equal(_testMessageEntity.AuthorId, context.Messages.Single().AuthorId);
            }
        }

        [Fact]
        public async Task Find_EntityByPK()
        {
            using (var context = GetInMemoryContext())
            {
                // Arrange
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());
                const ulong id = 333;

                _testMessageEntity.Id = id;
                context.Messages.Add(_testMessageEntity);
                await context.SaveChangesAsync();

                // Act
                var entity = await entityService.Find(id);

                // Assert
                Assert.Single(context.Messages);
                Assert.Equal(id, entity.Id);
            }
        }

        [Fact]
        public async Task Get_OneEntityWithIncludes()
        {
            using (var context = GetInMemoryContext())
            {
                // Arrange
                var entityService =
                    new EntityService<RemembotSettings>(context, new EntityChangedDispatcher<RemembotSettings>());
                const ulong guildID = 123;

                context.ReminderSettings.Add(new RemembotSettings
                {
                    GuildId = guildID,
                    RecurringReminders = new List<RecurringReminderMessage>
                    {
                        new RecurringReminderMessage
                        {
                            Message = "TestMessage"
                        }
                    }
                });
                await context.SaveChangesAsync();

                // Act
                var entity = await entityService.Get(r => r.GuildId == guildID, r => r.RecurringReminders);

                // Assert
                Assert.Single(context.RecurringReminderMessages);
                Assert.Equal(guildID, entity.First().GuildId);
                Assert.Equal("TestMessage", entity.First().RecurringReminders.First().Message);
            }
        }

        [Fact]
        public async Task GetAll()
        {
            using (var context = GetInMemoryContext())
            {
                // Arrange
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());

                context.Messages.Add(_testMessageEntity);
                await context.SaveChangesAsync();

                // Act
                var entity = await entityService.GetAll();

                // Assert
                Assert.Single(context.Messages);
                Assert.Single(entity);
            }
        }

        [Fact]
        public async Task Remove_OneEntity()
        {
            using (var context = GetInMemoryContext())
            {
                // Arrange
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());

                context.Messages.Add(_testMessageEntity);
                await context.SaveChangesAsync();

                // Act
                await entityService.Remove(_testMessageEntity);

                // Assert
                Assert.Equal(0, context.Messages.Count());
            }
        }

        [Fact]
        public async Task Update_OneEntity()
        {
            using (var context = GetInMemoryContext())
            {
                // Arrange
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());
                const ulong id = 999;

                context.Messages.Add(_testMessageEntity);
                await context.SaveChangesAsync();

                // Act
                _testMessageEntity.AuthorId = id;
                await entityService.Update(_testMessageEntity);

                // Assert
                Assert.Single(context.Messages);
                Assert.Equal(id, context.Messages.Single().AuthorId);
            }
        }

        [Fact]
        public async Task Caching_Entity_Service_PK()
        {
            using (var context = GetInMemoryContext())
            {
                var cache = new FluentDictionaryCache();

                var entityService = new CachedEntityService<RecurringReminderMessage>(context, new EntityChangedDispatcher<RecurringReminderMessage>(), cache);

                var reminder = new RecurringReminderMessage
                {
                    Message = "Test Message",
                    Id = 1,
                    Fault = RecurringReminderMessage.FaultType.None,
                    Enabled = true,
                    GuildId = 100,
                    ChannelId = 1,
                    CronExpression = "* * * * *"
                };

                await entityService.Create(reminder);

                var pkReminder = await entityService.Find(reminder.Id);

                var key = CachedEntityService<RecurringReminderMessage>.GetCacheKey(new object[] { reminder.Id });

                var cacheEntry = cache.WithKey(key).Get<RecurringReminderMessage>();

                Assert.True(cacheEntry.Value != null);
                Assert.True(cacheEntry.Value.Id == pkReminder.Id);
                Assert.True(cacheEntry.Value.Message == "Test Message");

                pkReminder.Message = "Test Message 2";
                await entityService.Update(pkReminder);

                cacheEntry = cache.WithKey(key).Get<RecurringReminderMessage>();

                Assert.True(cacheEntry.Value == null);

                var lastCheck = await entityService.Find(reminder.Id);

                Assert.True(lastCheck.Message == "Test Message 2");
            }
        }

        [Fact]
        public async Task Caching_Entity_Service_Expression()
        {
            using (var context = GetInMemoryContext())
            {
                var cache = new FluentDictionaryCache();

                var entityService = new CachedEntityService<RecurringReminderMessage>(context,
                    new EntityChangedDispatcher<RecurringReminderMessage>(), cache);

                var reminder = new RecurringReminderMessage
                {
                    Message = "Test Message",
                    Id = 1,
                    Fault = RecurringReminderMessage.FaultType.None,
                    Enabled = true,
                    GuildId = 100,
                    ChannelId = 1,
                    CronExpression = "* * * * *"
                };

                await entityService.Create(reminder);

                Expression<Func<RecurringReminderMessage, bool>> predicate = r => r.Id == reminder.Id;

                var pkReminder = ( await entityService
                        .Get(predicate) )
                    .FirstOrDefault();

                var key = entityService.GetCacheKey(predicate, null);

                var cacheEntry = cache.WithKey(key)
                    .Get<List<RecurringReminderMessage>>();

                Assert.True(cacheEntry.Value != null);
                Assert.True(cacheEntry.Value[0].Id == pkReminder.Id);
                Assert.True(cacheEntry.Value[0].Message == "Test Message");

                pkReminder.Message = "Test Message 2";
                await entityService.Update(pkReminder);

                cacheEntry = cache.WithKey(key)
                    .Get<List<RecurringReminderMessage>>();

                Assert.True(cacheEntry.Value == null);

                var lastCheck = ( await entityService
                    .Get(predicate) )
                    .FirstOrDefault();

                Assert.True(lastCheck?.Message == "Test Message 2");
            }
        }
    }
}