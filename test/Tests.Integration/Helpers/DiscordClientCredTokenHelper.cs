using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Tests.Integration.Helpers
{
    public class DiscordClientCredTokenHelper
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly string clientId;
        private readonly string clientSecret;
        private const string DiscordTokenEndpoint = "https://discordapp.com/api/oauth2/token";


        public DiscordClientCredTokenHelper(IConfiguration configuration)
        {
            clientId = configuration["Discord:ClientID"];
            clientSecret = configuration["Discord:ClientSecret"];

        }

        public async Task<string> GetToken()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(DiscordTokenEndpoint),
                Content = new StringContent("grant_type=client_credentials")
            };

            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

            var response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(data)["access_token"].ToString();

            return token;
        }
    }
}
