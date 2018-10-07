using Exico.Shopify.Web.Core.Filters;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// Extend this abstract controller if you want to limit access only to administrators.
    /// It restricts by admin role and privileged ips (saved in settings table in the db)
    /// <see cref="IPAddressVerification"/>
    /// <see cref="ABaseAuthorizedController"/>
    /// </summary>
    [Authorize(Roles = UserInContextHelper.ADMIN_ROLE)]
    [ServiceFilter(typeof(IPAddressVerification),Order = IPAddressVerification.DEFAULT_ORDER)]
    public abstract class ABaseAdminController : ABaseAuthorizedController
    {
        public ABaseAdminController( IUserCaching cachedUser, IConfiguration config, IDbSettingsReader settings,ILogger logger) : base(cachedUser, config,  settings,logger)
        {

        }
    }
}