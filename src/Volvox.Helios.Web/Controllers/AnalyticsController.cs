using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Domain.Module.ChatTracker;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.Analytics;
using System.Linq;

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

        public async Task<IActionResult> Index(ulong guildId)
        {
            var messages = await _messageEntityService.Get(message => message.GuildId == guildId);

            var uniqueUsers = messages.Select(m => m.AuthorId).Distinct().Count();

            var viewModel = new AnalyticsViewModel
            {
                ChatTracker = new ChatTrackerViewModel
                {
                    Messages = messages,
                    UniqueUsers = uniqueUsers
                }
            };

            return View(viewModel);
        }

        [HttpGet("ChatTracker")]
        public async Task<IActionResult> ChatTracker(ulong guildId)
        {
            var messages = await _messageEntityService.Get(message => message.GuildId == guildId);

            var uniqueUsers = messages.Select(m => m.AuthorId).Distinct().Count();

            var viewModel = new ChatTrackerViewModel
            {
                Messages = messages,
                UniqueUsers = uniqueUsers
            };

            return View(viewModel);
        }
    }
}