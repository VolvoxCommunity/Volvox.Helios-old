
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Web.Filters
{
    public class ClearCacheAfterPostFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {    
            if (context.HttpContext.Request.Method == "GET")
                return;

            if (context.ModelState.TryGetValue("guildId", out var guildIdEntry))
            {
                var guildIdRaw = guildIdEntry.RawValue as string;
                if (ulong.TryParse(guildIdRaw, out var guildId))
                {
                    var settings = context.HttpContext.RequestServices .GetService<IModuleSettingsService<ModerationSettings>>();

                    settings.ClearCacheByGuild(guildId);
                }
                else
                { 
                   context.Result = new BadRequestObjectResult(context.ModelState); 
                }
            }
            else
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
