using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Domain.Module.ChatTracker;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.Analytics;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/Analytics/{guildId}")]
    [IsUserGuildAdminFilter]
    public class AnalyticsController : Controller
    {
        private readonly IEntityService<Message> _messageEntityService;

        public AnalyticsController(IEntityService<Message> messageEntityService)
        {
            _messageEntityService = messageEntityService;
        }

        public IActionResult Index(ulong guildId)
        {
            return View();
        }

        [HttpGet("ChatTracker")]
        public async Task<IActionResult> ChatTracker(ulong guildId)
        {
            var messages = await _messageEntityService.Get(message => message.GuildId == guildId);

            var viewModel = new ChatTrackerViewModel
            {
                Messages = messages
            };

            return View(viewModel);
        }
    }
}