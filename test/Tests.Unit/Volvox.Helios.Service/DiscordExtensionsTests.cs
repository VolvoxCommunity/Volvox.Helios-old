using System.Collections.Generic;
using Volvox.Helios.Domain.Discord;
using Volvox.Helios.Service.Extensions;
using Xunit;

namespace Tests.Unit.Volvox.Helios.Service
{
    /// <summary>
    /// Q&D sample unit test class
    /// </summary>
    public class DiscordExtensionsTests
    {
        private const int AdminPermission = 0x00000008;
        private const int NotAdminPermission = 0x00000000;

        /// <summary>
        /// Sample Test
        /// </summary>
        [Fact]
        public void ReturnFilteredList()
        {
            //Arrange 
            #region arrange
            var testGuilds = new List<Guild>
            {
                new Guild
                {
                    Id = 1,
                    Name = "NotAdmin",
                    Permissions = NotAdminPermission
                },

                new Guild
                {
                    Id = 1,
                    Name = "NotAdmin2",
                    Permissions = NotAdminPermission
                },

                new Guild
                {
                    Id = 1,
                    Name = "Admin",
                    Permissions = AdminPermission
                }
            };
            #endregion

            //Act
            var result = testGuilds.FilterAdministrator();

            //Assert
            Assert.True(result.Count == 1);
            Assert.Equal("Admin", result[0].Name);
        }
    }
}
