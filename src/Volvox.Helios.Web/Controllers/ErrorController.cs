using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Volvox.Helios.Web.Controllers
{
    public class ErrorController : Controller
    {
        public static string GetStatusDescription(int code)
        {
            switch (code)
            {
                case 400: return "Bad Request";
                case 401: return "Unauthorized";
                case 402: return "Payment Required";
                case 403: return "Forbidden";
                case 405: return "Method Not Allowed";
                case 406: return "Not Acceptable";
                case 407: return "Proxy Authentication Required";
                case 408: return "Request Timeout";
                case 409: return "Conflict";
                case 410: return "Gone";
                case 411: return "Length Required";
                case 412: return "Precondition Failed";
                case 413: return "Request Entity Too Large";
                case 414: return "Request-Uri Too Long";
                case 415: return "Unsupported Media Type";
                case 416: return "Requested Range Not Satisfiable";
                case 417: return "Expectation Failed";
                case 422: return "Unprocessable Entity";
                case 423: return "Locked";
                case 424: return "Failed Dependency";
                case 501: return "Not Implemented";
                case 502: return "Bad Gateway";
                case 503: return "Service Unavailable";
                case 504: return "Gateway Timeout";
                case 505: return "Http Version Not Supported";
                case 507: return "Insufficient Storage";

                default: return "Internal Server Error";
            }
        }

        /// <summary>
        ///  Put custom error pages here.
        ///  Add new case in switch block for new non-generic error page.
        /// </summary>
        [HttpGet]
        public IActionResult Errors(int Id)
        {
            switch (Id)
            {
                case 404:
                    return View($"~/Views/Shared/Errors/Err{Id}.cshtml");
                default:
                    return Error(Id);
            }
        }

        [HttpGet]
        public IActionResult Error(int? Id)
        {
            int errorCode = Id.HasValue ? Id.Value : 500;
            ViewData["StatusCode"] = errorCode;
            ViewData["StatusDescription"] = GetStatusDescription(errorCode);
            return View("~/Views/Shared/Errors/ErrGeneral.cshtml");
        }
    }
}