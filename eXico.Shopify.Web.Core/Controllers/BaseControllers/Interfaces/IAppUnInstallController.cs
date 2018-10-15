using Exico.Shopify.Data.Domain.AppModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public enum UNINSTALL_ACTIONS
    {
        AppUninstalled,
        UnInstallCompleted,
        UserIsRemoved,
        CouldNotDeleteUser,
        SendUninstallEmail
    }

    public interface IAppUnInstallController
    {
        Task<IActionResult> AppUninstalled(string userId);
        Task<bool> SendUninstallEmail(AppUser user);
        Task UnInstallCompleted(AppUser user);
        Task UserIsDeleted(AppUser user);
        Task CouldNotDeleteUser(AppUser user, Exception ex);
    }
}