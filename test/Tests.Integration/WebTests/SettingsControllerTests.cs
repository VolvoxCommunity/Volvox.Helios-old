using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tests.Integration.Helpers;
using Tests.Integration.Infrastructure;
using Volvox.Helios.Domain.Discord;
using Xunit;

namespace Tests.Integration.WebTests
{
    public class SettingsControllerTests : IClassFixture<CustomWebApplicationFactory<Volvox.Helios.Web.Startup>>
    {
        private readonly CustomWebApplicationFactory<Volvox.Helios.Web.Startup> _webApplicationFactory;

        public SettingsControllerTests(CustomWebApplicationFactory<Volvox.Helios.Web.Startup> customWebApplicationFactory)
        {
            _webApplicationFactory = customWebApplicationFactory;
        }

        [Theory]
        [InlineData("/Settings")]
        public async Task Get_Default(string url)
        {
            var client = _webApplicationFactory.CreateClient();
            var credBytes = Encoding.UTF8.GetBytes("testuser:testpassword");
            var b64Creds = Convert.ToBase64String(credBytes);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", b64Creds);
            var testServerName = ConfigurationHelper.Configuration["Discord:TestServerName"];

            var guildsRequest = await client.GetAsync("/api/GetUserAdminGuilds");

            var guildsResponse = await guildsRequest.Content.ReadAsStringAsync();
            var guilds = JsonConvert.DeserializeObject<List<Guild>>(guildsResponse);

            var testGuildId = guilds.Where(g => g.Name.Equals(testServerName)).Select(g => g.Id).FirstOrDefault();

            var response = await client.GetAsync($"{url}/{testGuildId}");

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

        }

    }
}
