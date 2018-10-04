using System;
using System.Threading.Tasks;
using Volvox.Helios.Service.Clients;

namespace Tests.Integration.Infrastructure.TestServices
{
    class TestDiscordApiClient : IDiscordAPIClient
    {
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
            throw new NotImplementedException();
        }
    }
}
