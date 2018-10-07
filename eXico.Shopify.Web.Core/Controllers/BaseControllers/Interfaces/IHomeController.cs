using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public enum HOME_ACTIONS
    {
        Index
    }
    public interface IHomeController
    {
        Task<ActionResult> Index();
        [Route("/Error")]
        Task<IActionResult> Error();
    }
}
