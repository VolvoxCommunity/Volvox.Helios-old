using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Bot.Connector;
using Volvox.Helios.Core.Modules.Common;

namespace Tests.Integration.Infrastructure.TestServices
{
    class TestBot : IBot
    {
        public DiscordSocketClient Client => throw new NotImplementedException();

        public IBotConnector Connector => throw new NotImplementedException();

        public IList<IModule> Modules => throw new NotImplementedException();

        public ILogger<Bot> Logger => throw new NotImplementedException();

        public int GetBotRoleHierarchy(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<SocketGuild> GetGuilds()
        {
            throw new NotImplementedException();
        }

        public Task InitModules()
        {
            throw new NotImplementedException();
        }

        public bool IsBotInGuild(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public Task Log(LogMessage message)
        {
            throw new NotImplementedException();
        }

        public Task Start()
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
