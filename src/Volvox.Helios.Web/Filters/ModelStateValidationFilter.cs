using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Volvox.Helios.Web.Filters
{
    public class ModelStateValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid) context.Result = new BadRequestObjectResult(context.ModelState);

            base.OnActionExecuting(context);
        }
    }
}