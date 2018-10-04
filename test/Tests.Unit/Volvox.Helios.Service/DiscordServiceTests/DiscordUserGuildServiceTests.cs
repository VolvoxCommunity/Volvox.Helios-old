using System.Threading.Tasks;
using Moq;
using Volvox.Helios.Service.Clients;
using Volvox.Helios.Service.Discord.UserGuild;
using Xunit;

namespace Tests.Unit.Volvox.Helios.Service.DiscordServiceTests
{
    public class DiscordUserGuildServiceTests
    {
        [Fact]
        public async Task GetUserGuilds()
        {
            // Arrange
            var discordApiClient = new Mock<DiscordAPIClient>();
            discordApiClient.Setup(d => d.GetUserGuilds()).ReturnsAsync(
                "[{\"id\":\"468467000344313866\",\"name\":\"Volvox\",\"icon\":\"72225e0911dc450048250a6b28dfab5b\"},{\"id\":\"471503658694213632\",\"name\":\"BapesTestServer\",\"icon\":null}]");

            var userGuildService = new DiscordUserGuildService(discordApiClient.Object);

            // Act
            var guilds = await userGuildService.GetUserGuilds();

            //Assert
            discordApiClient.Verify(d => d.GetUserGuilds(), Times.AtMostOnce);
            Assert.True(guilds.Count == 2);
            Assert.Equal("Volvox", guilds[0].Guild.Name);
            Assert.Equal("BapesTestServer", guilds[1].Guild.Name);
        }
    }
}