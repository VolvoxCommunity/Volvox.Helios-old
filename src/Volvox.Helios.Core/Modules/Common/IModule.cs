using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.DiscordFacing;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    ///     Unit of the bot.
    /// </summary>
    public interface IModule
    {
        void Enable();

        void Disable();

        /// <summary>
        ///     Executes the module.
        /// </summary>
        Task InvokeAsync(DiscordFacingContext discordFacingContext);
    }
}