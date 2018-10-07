using System;
using System.Threading.Tasks;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ViewModels;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// Interface for implementing email sending functionalities for different app events.
    /// The default implementation in <see cref="ShopifyEventsEmailer"/>
    /// </summary>
    public interface IShopifyEventsEmailer
    {
        Task<bool> ErrorOccurredAsync(AppUser user, Exception ex);
        Task<bool> InActiveChargeIdDetectedAsync(AppUser user, string chargeIdStatus);
        Task<bool> SendSupportEmailAsync(AppUser user, ContactUsViewModel model);
        Task<bool> UninstallHookCreationFailedAsync(AppUser user, string shop);
        Task<bool> UserInstalledAppAsync(AppUser user);
        Task<bool> UserPaymentInfoCouldNotBeSavedAsync(AppUser user, long chargeId, string planName);
        Task<bool> UserUnInstalledAppAsync(AppUser user);
        Task<bool> UserUpgradedPlanAsync(AppUser user, int newPlainId);
        Task<bool> UserWelcomeEmailAsync(string sendTo);
    }
}