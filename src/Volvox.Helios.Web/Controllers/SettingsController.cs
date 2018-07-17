using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Service.Discord;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    public class SettingsController : Controller
    {
        // TODO: Return all of the modules
        public IActionResult Index()
        {
            return View();
        }

        // GET
        public async Task<IActionResult> StreamAnnouncerSettings([FromServices] IDiscordUserService userService)
        {
            var guilds = await userService.GetUserGuilds();
            
            var viewModel = new StreamAnnouncerSettingsViewModel()
            {
                Guilds = new SelectList(guilds.FilterAdministrator(), "Id", "Name")
            };
            
            return View(viewModel);
        }
        
        // POST
        [HttpPost]
        public async Task<IActionResult> StreamAnnouncerSettings(StreamAnnouncerSettingsViewModel viewModel)
        {
            return Index();
        }
    }
}