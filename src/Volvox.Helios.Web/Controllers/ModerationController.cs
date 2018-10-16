using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Web.Controllers
{
    public class ModerationController : Controller
    {
        IModuleSettingsService<ModerationSettings> _moderationSettings;

        IEntityService<ProfanityFilter> _entityServiceProfanityFilter;

        public ModerationController(IModuleSettingsService<ModerationSettings> moderationSettings, IEntityService<ProfanityFilter> entityServiceProfanityFilter)
        {
            _moderationSettings = moderationSettings;

            _entityServiceProfanityFilter = entityServiceProfanityFilter;
        }

        //public async Task<IActionResult> Index(ulong guildId)
        //{

        //}
    }
}