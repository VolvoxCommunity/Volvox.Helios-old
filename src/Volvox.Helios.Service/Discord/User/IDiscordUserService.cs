using System.Threading.Tasks;

namespace Volvox.Helios.Service.Discord.User
{
    public interface IDiscordUserService
    {
        /// <summary>
        ///     Get the specified user.
        /// </summary>
        /// <param name="userId">Id of the user to get.</param>
        /// <returns>Specified user.</returns>
        Task<Domain.Discord.User> GetUser(ulong userId);
    }
}