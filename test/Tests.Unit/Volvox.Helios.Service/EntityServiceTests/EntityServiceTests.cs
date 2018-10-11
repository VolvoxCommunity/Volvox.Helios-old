using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            // Arrange
            using (var context = GetInMemoryContext())
            {
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());

                // Act
                await entityService.Create(_testMessageEntity);

                // Assert
                Assert.Single(context.Messages);
                Assert.Equal((ulong)123, context.Messages.Single().AuthorId);
            }
        }

        [Fact]
        public async Task Find_EntityByPK()
        {
            // Arrange
            using (var context = GetInMemoryContext())
            {
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());

                _testMessageEntity.Id = 333;
                context.Messages.Add(_testMessageEntity);
                await context.SaveChangesAsync();

                // Act
                var entity = await entityService.Find((ulong)333);

                // Assert
                Assert.Single(context.Messages);
                Assert.Equal((ulong)333, entity.Id);
            }
        }

        [Fact]
        public async Task Get_OneEntityWithIncludes()
        {
            // Arrange
            using (var context = GetInMemoryContext())
            {
                var entityService =
                    new EntityService<RemembotSettings>(context, new EntityChangedDispatcher<RemembotSettings>());

                context.ReminderSettings.Add(new RemembotSettings
                {
                    GuildId = 123,
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
                var entity = await entityService.Get(r => r.GuildId == 123, r => r.RecurringReminders);

                // Assert
                Assert.Single(context.RecurringReminderMessages);
                Assert.Equal((ulong)123, entity.First().GuildId);
                Assert.Equal("TestMessage", entity.First().RecurringReminders.First().Message);
            }
        }

        [Fact]
        public async Task GetAll()
        {
            // Arrange
            using (var context = GetInMemoryContext())
            {
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
            // Arrange
            using (var context = GetInMemoryContext())
            {
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
            // Arrange
            using (var context = GetInMemoryContext())
            {
                var entityService = new EntityService<Message>(context, new EntityChangedDispatcher<Message>());

                context.Messages.Add(_testMessageEntity);
                await context.SaveChangesAsync();

                // Act
                _testMessageEntity.AuthorId = 999;
                await entityService.Update(_testMessageEntity);

                // Assert
                Assert.Single(context.Messages);
                Assert.Equal((ulong)999, context.Messages.Single().AuthorId);
            }
        }
    }
}