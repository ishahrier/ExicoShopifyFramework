using Exico.Shopify.Data.Domain.ViewModels;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Helpers
{
    public static class ViewHelper
    {
        public static ViewNames GetViews(this IRazorPage rpage)
        {
            var vdata = rpage.ViewContext.ViewBag.Views as ViewNames;
            return vdata;
        }

        public static IHtmlContent GetFrameWorkVersionAndBuild(this IRazorPage rpage)
        {
            var vdata = rpage.ViewContext.ViewBag.VersionInfo as Versions;
            return new HtmlString($"<b>ver.</b>{vdata.FrameWorkVersion}");
        }
        public static string GetAppNameAndVersion(this IRazorPage rpage)
        {
            var vdata = rpage.ViewContext.ViewBag.VersionInfo as Versions;
            return $"{rpage.ViewContext.ViewBag.AppName} - ver.{vdata.AppVersion}";
        }

        public static ControllerNames GetControllers(this IRazorPage rpage)
        {
            var vdata = rpage.ViewContext.ViewBag.Controllers as ControllerNames;
            return vdata;
        }

        public static bool ViewExists(this IRazorPage rpage, ICompositeViewEngine viewEngine, string viewName)
        {
            var result = viewEngine.FindView(rpage.ViewContext, viewName, false);
            return result.Success;
        }

        public static async Task<IHtmlContent> PartialAsyncIfExists(this IHtmlHelper html, IRazorPage rpage, ICompositeViewEngine viewEngine, string viewName, object model = null)
        {
            if (rpage.ViewExists(viewEngine, viewName))
            {
                return await html.PartialAsync(viewName, model, rpage.ViewContext.ViewData);
            }
            else
            {
                return HtmlString.Empty;
            }
        }

        public static IHtmlContent PrintViewNameAttr(this IRazorPage rpage)
        {
            if (rpage.ViewContext.ViewBag.PrintViewFileName == "1")
                return new HtmlString($"x-viewname='{rpage.ViewContext.ExecutingFilePath}'");
            else
                return HtmlString.Empty;
        }
    }
}
