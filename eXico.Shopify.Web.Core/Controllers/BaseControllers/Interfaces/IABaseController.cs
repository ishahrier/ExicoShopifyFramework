using Microsoft.AspNetCore.Mvc.Filters;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public interface IABaseController : IResultFilter
    {
        void MyOnResultExecuted(ResultExecutedContext contex);
        void MyOnResultExecuting(ResultExecutingContext contex);
        void MyOnActionExecuting(ActionExecutingContext context);
        void MyOnActionExecuted(ActionExecutedContext context);

    }
}