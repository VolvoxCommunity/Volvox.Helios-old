using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Domain.User;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Web.Controllers
{
    [Route("/api/user")]
    [Authorize]
    public class UserSettingsController : Controller
    {
        private readonly CachedEntityService<UserSettings> _userSettingsService;

        public UserSettingsController(CachedEntityService<UserSettings> userSettingsService)
        {
            _userSettingsService = userSettingsService;
        }

        public async Task<UserSettings> GetUserSettings()
        {
            var userId = GetUserId();

            return await _userSettingsService.Find(userId);
        }

        private ulong GetUserId()
        {
            if (User == null)
                throw new Exception("User is not defined.");

            var parseResult = ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            // Throw an exception if the user id could not be parsed
            if (!parseResult) throw new Exception("Cannot parse user id!");

            return userId;
        }
    }
}