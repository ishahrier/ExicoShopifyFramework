using System.Threading.Tasks;
using Exico.Shopify.Data.Domain.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public enum PROFILE_ACTIONS
    {
        ChangePlan,      
        Index
    }
    public interface IMyProfileController
    {
        Task<ActionResult> Index();
        
        Task<ActionResult> ChangePlan(bool proceed = false);
    }
}