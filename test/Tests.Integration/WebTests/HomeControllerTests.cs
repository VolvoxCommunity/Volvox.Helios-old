using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.WebTests
{
    /// <summary>
    /// Q&D integration test class for testing the webapp
    /// </summary>
    public class HomeControllerTests : IClassFixture<WebApplicationFactory<Volvox.Helios.Web.Startup>>
    {
        private readonly WebApplicationFactory<Volvox.Helios.Web.Startup> _webApplicationFactory;

        public HomeControllerTests(WebApplicationFactory<Volvox.Helios.Web.Startup> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        /// <summary>
        /// Quick Test against / to ensure 200 OK unauthed
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [Theory]
        [InlineData("/")]
        public async Task Get_Default(string url)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }
    }
}
