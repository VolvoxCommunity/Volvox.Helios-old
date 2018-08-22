using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/api")]
    public class ApiController : Controller
    {
        // GET
        [HttpGet("GetGuildChannels")]
        public async Task<object> GetGuildChannels([FromServices] IDiscordGuildService guildService, ulong guildId)
        {
            var channels = await guildService.GetChannels(guildId);

            // Format the ulong to string.
            return channels.FilterChannelType(0).Select(c => new {id = c.Id.ToString(), name = c.Name});
        }
        
        [HttpGet("GetUserAdminGuilds")]
        public async Task<object> GetUserAdminGuilds([FromServices] IDiscordUserGuildService userGuildService)
        {
            var guilds = await userGuildService.GetUserGuilds();

            // Format the ulong to string.
            return guilds.FilterAdministrator().Select(g => new {id = g.Guild.Id.ToString(), name = g.Guild.Name});
        }

        [HttpGet("IsBotInGuild")]
        public bool IsBotInGuild(ulong guildId, [FromServices] IBot bot)
        {
            return bot.IsBotInGuild(guildId);
        }
    }
}