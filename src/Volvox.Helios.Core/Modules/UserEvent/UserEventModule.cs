using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.StreamerRole;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.UserEvent
{
    public class UserEventModule : Module
    {
        /// <summary>
        ///     Perform actions based on a user event.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="config">Application configuration.</param>
        public UserEventModule(IDiscordSettings discordSettings, ILogger<StreamerRoleModule> logger,
            IConfiguration config) : base(discordSettings, logger, config)
        {
        }

        /// <summary>
        ///     Initialize the module.
        /// </summary>
        /// <param name="client">Client that the module will be registered to.</param>
        public override Task Init(DiscordSocketClient client)
        {
            return Task.CompletedTask;
        }
    }
}