using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.Integration.Infrastructure;
using Xunit;

namespace Tests.Integration.WebTests
{
    public class SecureTest : IClassFixture<CustomWebApplicationFactory<Volvox.Helios.Web.Startup>>
    {
        private readonly CustomWebApplicationFactory<Volvox.Helios.Web.Startup> _webApplicationFactory;

        public SecureTest(CustomWebApplicationFactory<Volvox.Helios.Web.Startup> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        /// <summary>
        /// Quick Test against / to ensure 200 OK unauthed
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [Theory]
        [InlineData("/SecureTest")]
        public async Task Get_Default(string url)
        {
            var client = _webApplicationFactory.CreateClient();
            var credBytes = Encoding.UTF8.GetBytes("testuser:testpassword");
            var b64Creds = Convert.ToBase64String(credBytes);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", b64Creds);

            var response = await client.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

    }
}
