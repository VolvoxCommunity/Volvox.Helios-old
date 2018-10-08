using System;
using System.Threading.Tasks;
using Volvox.Helios.Service.Clients;

namespace Tests.Integration.Infrastructure.TestServices
{
    public class TestDiscordApiClient : IDiscordAPIClient
    {
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
