using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Exico.Shopify.Web.Core.Filters
{
    public class AdminPasswordVerification : IActionFilter
    {
        public const string ADMIN_PASS_KEYT = "AdminPassKey";
        public const string ADMIN_PASS_VALUE = "AdminPassVal";
        private string _Password, _QueryParam;
        private readonly ILogger<AdminPasswordVerification> _Logger;

        public AdminPasswordVerification(IConfiguration config, ILogger<AdminPasswordVerification> logger) : base()
        {
            this._QueryParam = config[ADMIN_PASS_KEYT];
            this._Password = config[ADMIN_PASS_VALUE];
            this._Logger = logger;

        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _Logger.LogInformation("Varifying admin password.");
            var pass = context.HttpContext.Request.Query[_QueryParam];
            if (pass == _Password)
                _Logger.LogInformation("Valid admin password detected.Letting in.");
            else
            {
                _Logger.LogWarning("Invalid admin password detected.Returning UnauthorizedResult.");
                context.Result = new UnauthorizedResult() { };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
