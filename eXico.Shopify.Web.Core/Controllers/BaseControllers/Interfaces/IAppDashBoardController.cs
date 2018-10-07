using System.Threading.Tasks;
using Exico.Shopify.Data.Domain.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public enum DASHBOARD_ACTIONS
    {
        Index,
        ConsiderUpgradingPlan,
        PlanDoesNotAllow,
        Support,
        SendMsg
    }
    public interface IAppDashBoardController
    {
        
        Task<IActionResult> Index();
        Task<IActionResult> ConsiderUpgradingPlan();
        Task<IActionResult> PlanDoesNotAllow();
        Task<IActionResult> Support();
        Task<IActionResult> SendMsg(ContactUsViewModel model);
    }
}