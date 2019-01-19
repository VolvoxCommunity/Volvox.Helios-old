using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Volvox.Helios.Domain.ModuleSettings;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Web.Filters
{
    public class EnsureModerationSettingsEntryExistsFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.TryGetValue("guildId", out var guildId))
            {
                var settings = context.HttpContext.RequestServices.GetService<IModuleSettingsService<ModerationSettings>>();

                if (await settings.GetSettingsByGuild((ulong)guildId) == null)
                {
                    await settings.SaveSettings(new ModerationSettings {GuildId = (ulong)guildId});
                }
            }

            else
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }

            await next();
        }
    }
}
