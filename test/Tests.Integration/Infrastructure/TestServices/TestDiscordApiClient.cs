using System;
using System.Net.Http;
using System.Threading.Tasks;
using Volvox.Helios.Service.Clients;

namespace Tests.Integration.Infrastructure.TestServices
{
    public class TestDiscordApiClient : IDiscordAPIClient
    {
        private readonly HttpClient _fakeClient; //TODO: probably remove this, or do some different DI

        public TestDiscordApiClient(HttpClient client)
        {
            //Needed for the DI from addhttpclient()
            _fakeClient = client;
        }

        public Task<string> GetGuildChannels(ulong guildId)
        {
            return Task.FromResult("[{\"id\":\"468467000344313866\",\"name\":\"Volvox\",\"icon\":\"72225e0911dc450048250a6b28dfab5b\"},{\"id\":\"471503658694213632\",\"name\":\"BapesTestServer\",\"icon\":null}]");
        }

        public Task<string> GetGuildRoles(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserGuilds()
        {
            throw new NotImplementedException();
        }
    }
}
