using System;
using System.Threading.Tasks;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    /// Unit of the bot.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Start the module.
        /// </summary>
        Task Start();

        /// <summary>
        /// Execute the module.
        /// </summary>
        Task Execute();
    }
}