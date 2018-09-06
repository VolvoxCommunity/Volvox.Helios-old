using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Volvox.Helios.Web.Controllers
{
    public class SecureTestController : Controller
    {
        [Authorize]
        public string Index()
        {
            var claims = HttpContext.User.Claims;
            var strbuilder = new StringBuilder();

            foreach (var claim in claims)
            {
                strbuilder.Append($"{claim.Type} = {claim.Value}");
            }

            return strbuilder.ToString();
        }
    }
}