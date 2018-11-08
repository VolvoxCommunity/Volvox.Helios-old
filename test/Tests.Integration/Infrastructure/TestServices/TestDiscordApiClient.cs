using System;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Integration.Helpers;
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
            throw new NotImplementedException();
        }

        public Task<string> GetGuildRoles(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserGuilds()
        {
            return Task.FromResult(TestData.TestGuildsDataResponse);
        }
    }
}
