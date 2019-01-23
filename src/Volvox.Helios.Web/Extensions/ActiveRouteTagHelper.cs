using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Volvox.Helios.Web.Extensions
{
    [HtmlTargetElement(Attributes = "asp-is-active")]
    public class ActiveRouteTagHelper : TagHelper
    {
        /// <summary>
        ///     Name of the action method.
        /// </summary>
        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }

        /// <summary>
        ///     Name of the controller.
        /// </summary>
        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; }

        /// <summary>
        ///     Gets the ViewContext for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext] public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("match-controller")]
        public bool MatchController { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ShouldBeActive()) MakeActive(output);

            // Remove identifying attribute
            output.Attributes.RemoveAll("asp-is-active");
        }

        private bool ShouldBeActive()
        {
            // TODO: Clean this up
            var routeData = ViewContext.RouteData.Values;

            var pageController = string.Empty;
            var pageAction = string.Empty;

            if (routeData.ContainsKey("page"))
            {
                var page = $"{routeData["page"]}".ToLowerInvariant();
                pageController = page.Substring(0, page.LastIndexOf('/')).Replace("/", string.Empty);
                pageAction = page.Substring(page.LastIndexOf('/')).Replace("/", string.Empty);
            }

            var currentController = ViewContext.RouteData.Values["Controller"]?.ToString() ?? pageController;
            var currentAction = ViewContext.RouteData.Values["Action"]?.ToString() ?? pageAction;

            if (MatchController && !string.IsNullOrWhiteSpace(Controller) && string.Equals(Controller,
                    currentController, StringComparison.CurrentCultureIgnoreCase))
                return true;

            if (!string.IsNullOrWhiteSpace(Controller) && !string.Equals(Controller, currentController,
                    StringComparison.CurrentCultureIgnoreCase)) return false;

            return string.IsNullOrWhiteSpace(Action) ||
                   string.Equals(Action, currentAction, StringComparison.CurrentCultureIgnoreCase);
        }

        private void MakeActive(TagHelperOutput output)
        {
            // Get the tag attribute
            var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");

            if (classAttr == null)
            {
                // Add new class tag with active attribute
                classAttr = new TagHelperAttribute("class", "active");
                output.Attributes.Add(classAttr);
            }
            else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf("active") < 0)
                output.Attributes.SetAttribute("class",
                    classAttr.Value == null ? "active" : classAttr.Value + " active");
        }
    }
}