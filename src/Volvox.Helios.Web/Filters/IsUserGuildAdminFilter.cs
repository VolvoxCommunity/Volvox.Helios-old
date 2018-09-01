using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;

namespace Volvox.Helios.Web.Filters
{
    /// <summary>
    ///     Returns an unauthorized result if the logged in user is not an admin of the selected guild.
    /// </summary>
    public class IsUserGuildAdminFilter : ActionFilterAttribute
    {
        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("guildId", out var guildId))
            {
                var userGuildService = context.HttpContext.RequestServices.GetService<IDiscordUserGuildService>();

                var userGuilds = await userGuildService.GetUserGuilds();

                if (userGuilds.FilterAdministrator().SingleOrDefault(g => g.Guild.Id == (ulong) guildId) != null)
                    base.OnActionExecuting(context);

                else
                    context.Result = new UnauthorizedResult();
            }

            else
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}