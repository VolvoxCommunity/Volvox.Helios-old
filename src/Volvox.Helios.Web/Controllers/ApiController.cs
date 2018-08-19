using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<JsonResult> GetGuildChannels([FromServices] IDiscordGuildService guildService, ulong guildId)
        {
            var channels = await guildService.GetChannels(guildId);

            // Format the ulong to string.
            return Json(channels.FilterChannelType(0).Select(c => new {id = c.Id.ToString(), name = c.Name}));
        }
        
        [HttpGet("GetUserAdminGuilds")]
        public async Task<JsonResult> GetUserAdminGuilds([FromServices] IDiscordUserGuildService userService)
        {
            var guilds = await userService.GetUserGuilds();

            // Format the ulong to string.
            return Json(guilds.FilterAdministrator().Select(g => new {id = g.Guild.Id.ToString(), name = g.Guild.Name}));
        }
    }
}