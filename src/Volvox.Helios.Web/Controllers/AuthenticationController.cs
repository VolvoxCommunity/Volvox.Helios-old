using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Volvox.Helios.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/signin")]
        public IActionResult SignIn()
        {
            var returnUrl = HttpContext.Request.Query["ReturnUrl"].ToString();

            if (
                   string.IsNullOrWhiteSpace(returnUrl)
                || !Url.IsLocalUrl(returnUrl)
                )
            {
                returnUrl = "/";
            }

            return Challenge(
                new AuthenticationProperties {RedirectUri =  returnUrl},
                "Discord");
        }

        [HttpGet("~/signout")]
        [HttpPost("~/signout")]
        public IActionResult SignOut()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow.
            return SignOut(new AuthenticationProperties {RedirectUri = "/"},
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}