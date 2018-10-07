using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.ViewModels;
using Exico.Shopify.Web.Core.Plugins.Email;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// A helper that uses <see cref="IEmailer"/> to send emails on different app realted events.
    /// </summary>
    /// <seealso cref="Exico.Shopify.Web.Core.Helpers.IShopifyEventsEmailer" />
    public class ShopifyEventsEmailer : IShopifyEventsEmailer
    {
        private readonly IEmailer _Emailer;
        private readonly IDbSettingsReader _Settings;
        private readonly ILogger<ShopifyEventsEmailer> _Logger;
        private readonly IPlansReader _PlanHelper;

        public ShopifyEventsEmailer(IEmailer emailer, IDbSettingsReader settings, IPlansReader planHelper, ILogger<ShopifyEventsEmailer> logger)
        {
            _Emailer = emailer;
            _Settings = settings;
            _Logger = logger;
            _PlanHelper = planHelper;
        }

        /// <summary>
        /// Sends email with related  user inforamtion when users the install the application.
        /// Emails are sent to event email subceribers set in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/>.</param>
        /// <returns></returns>
        public async Task<bool> UserInstalledAppAsync(AppUser user)
        {

            try
            {
                string msg = $"Hi <br/>";
                msg += $"{user.MyShopifyDomain} just <b style='color:green'>installed</b> the application. A welcome email has already been dispatched to the user.";
                msg += "<br/> Thanks <hr/>";
                msg += GetCurrentContextInHtml(user);
                _Emailer.SetMessage(msg, true);
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.AddTo(_Settings.GetShopifyEmailSubscriberList());
                _Emailer.SetSubject($"💓 Installation | {_Settings.GetAppName()} | {user.MyShopifyDomain}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending app isntall notification was successful.");
                else
                    _Logger.LogWarning("Sending app isntall notification was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error sending app install notification email.{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends email with related user inforamtion when users the uninstall the application.
        /// Emails are sent to event email subceribers set in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/>.</param>
        /// <returns></returns>
        public async Task<bool> UserUnInstalledAppAsync(AppUser user)
        {
            try
            {
                string msg = $"Hi <br/>";
                msg += $"{user.MyShopifyDomain} just <b style='color:red'>un-installed</b> the application.";
                msg += "<br/> Thanks <hr/>";
                msg += GetCurrentContextInHtml(user);
                _Emailer.AddTo(_Settings.GetShopifyEmailSubscriberList());
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.SetMessage(msg, true);
                _Emailer.SetSubject($"💔 Uninstallation | {_Settings.GetAppName()} | {user.MyShopifyDomain}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending UserUnInstalledApp notification was successful.");
                else
                    _Logger.LogWarning("Sending UserUnInstalledApp notification was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error Sending  plan upgrade notification email. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends email with related  user  inforamtion when users upgrades current plan.
        /// Includes associtated user information as well.Emails are sent to event email subceribers set in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/>.</param>
        /// <param name="newPlainId">The new plain identifier.</param>
        /// <returns></returns>
        public async Task<bool> UserUpgradedPlanAsync(AppUser user, int newPlainId)
        {
            try
            {
                var prev = _PlanHelper[user.PlanId.Value];
                var newP = _PlanHelper[newPlainId];
                string msg = $"Hi <br/>";
                msg += $"{user.UserName} just <b style='color:green'>UPGRADED</b> from plan {prev.Name} to plan {newP.Name}.";
                msg += "<br/> Thanks <hr/>";
                msg += GetCurrentContextInHtml(user);
                _Emailer.SetMessage(msg, true);
                _Emailer.AddTo(_Settings.GetShopifyEmailSubscriberList());
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.SetSubject($"★ Plan Upgraded | {_Settings.GetAppName()} | {user.MyShopifyDomain}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending app plan upgrade notification was successful.");
                else
                    _Logger.LogWarning("Sending app plan upgrade notification was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error plan upgrade notification email. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends email with related user  inforamtion when framework detects that user who is trying to use the app has invalid.
        /// Includes associtated user information as well.Emails are sent to event email subceribers set in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/>.</param>
        /// <param name="chargeIdStatus">The charge identifier status. i.e. Inactive</param>
        /// <returns></returns>
        public async Task<bool> InActiveChargeIdDetectedAsync(AppUser user, string chargeIdStatus)
        {
            try
            {
                string msg = $"Hi <br/>";
                msg += $"{user.UserName} has <b style='color:red'>inactive</b> charge status. Current status of the saved charge id is {chargeIdStatus}. ";
                msg += "<br/> Thanks <hr/>";
                msg += GetCurrentContextInHtml(user);
                _Emailer.SetMessage(msg, true);
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.AddTo(_Settings.GetShopifyEmailSubscriberList());
                _Emailer.SetSubject($"❗ Inactive Charge Detected | {_Settings.GetAppName() } | {user.MyShopifyDomain}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending inactive charge id detection notification was successful.");
                else
                    _Logger.LogInformation("Sending inactive charge id detection notification was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error sending inactive charge id notification email. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends email when users the payment information could not be saved.
        /// Includes associated user information as well.Emails are sent to event email subceribers set in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/>.</param>
        /// <param name="chargeId">The charge identifier, which is valid but not saved yet</param>
        /// <param name="planName">Name of the plan, valid, but not saved</param>
        /// <returns></returns>
        public async Task<bool> UserPaymentInfoCouldNotBeSavedAsync(AppUser user, long chargeId, string planName)
        {
            try
            {
                string msg = $"Hi <br/> Activated charge id is [{chargeId}] for plan [{planName}] for store {user.MyShopifyDomain}.";
                msg += $"But <b style='color:red'>could not save it in the database</b>.<br/>Thanks<hr/>" + GetCurrentContextInHtml(user);
                _Emailer.SetMessage(msg, true);
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.AddTo(_Settings.GetShopifyEmailSubscriberList());
                _Emailer.SetSubject($"⛔ Unsaved Payment Info | {_Settings.GetAppName() } | {user.MyShopifyDomain}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending unsaved payment notification was successful.");
                else
                    _Logger.LogInformation("Sending unsaved payment notification was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error sending unsaved payment notification email. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends email with associated user information when failed creating the uninstall hook.
        /// Emails are sent to event email subceribers set in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/>.</param>
        /// <param name="shop">The shop.</param>
        /// <returns></returns>
        public async Task<bool> UninstallHookCreationFailedAsync(AppUser user, string shop)
        {
            try
            {
                string msg = $"Hi <br/> Creating uninstall hook <b style='color:red'>failed</b> for shop {shop}.<br/>Thanks <hr/>";
                    
                msg += GetCurrentContextInHtml(user);
                _Emailer.SetMessage(msg, true);
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.AddTo(_Settings.GetShopifyEmailSubscriberList());
                _Emailer.SetSubject($"⛔ Uninstall Hook Creation Failed | {_Settings.GetAppName()} | {user.MyShopifyDomain}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending uninstall hook creation failure notification was successful.");
                else
                    _Logger.LogInformation("Sending uninstall hook creation failure notification was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error sending uninstall hook creation email. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Semail is sent with the error that occured in the framework, with associated user and exception details. 
        /// Emails are sent to event email subceribers set in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/>.</param>
        /// <param name="ex">The exception.</param>
        /// <returns></returns>
        public async Task<bool> ErrorOccurredAsync(AppUser user, Exception ex)
        {
            try
            {

                var msg = $"Hi <br/>";
                msg += $"<b style='color:red'>Error occurred</b> during execution for store {user.MyShopifyDomain}.<br/>Here is the exception details.<br/>";
                msg += $"<br/><pre>{ex.ToString()}</pre><br/><br/>Thanks <hr/><br/>";
                msg += GetCurrentContextInHtml(user);
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.SetMessage(msg, true);
                _Emailer.SetSubject($"⛔ Error Occurred | {_Settings.GetAppName()} | {user.MyShopifyDomain}");
                _Emailer.AddTo(_Settings.GetShopifyEmailSubscriberList());
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending error notification was successful.");
                else
                    _Logger.LogInformation("Sending error notification was unsuccessful.");
                return result;
            }
            catch (Exception e)
            {
                _Logger.LogError(ex, $"Error sending error notification email.{e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends this welcome email to user who just installed the app.
        /// The content can be set in the database using the settings admin panel.
        /// </summary>
        /// <param name="sendTo">Users email address</param>
        /// <returns></returns>
        public async Task<bool> UserWelcomeEmailAsync(string sendTo)
        {
            try
            {
                _Emailer.AddTo(sendTo);
                var welcomeMsg = _Settings.GetValue(CORE_SYSTEM_SETTING_NAMES.WELCOME_EMAIL_TEMPLATE);
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                _Emailer.SetMessage(welcomeMsg, true);
                _Emailer.SetSubject($"Welcome To {_Settings.GetAppName()}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending user welcome email was successful.");
                else
                    _Logger.LogInformation("Sending user welcome email was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error sending welcome email.{ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Sends the support email to the app admin.
        /// Emails are sent to support emails defined in the settings.
        /// </summary>
        /// <param name="user">The user <see cref="AppUser"/></param>
        /// <param name="model"><see cref="ContactUsViewModel"/></param>
        /// <returns></returns>
        public async Task<bool> SendSupportEmailAsync(AppUser user, ContactUsViewModel model)
        {
            try
            {
                string msg = model.Message.Replace("\r\n", "<br/>") + "<br/><hr/>" + GetCurrentContextInHtml(user);
                _Emailer.AddTo(_Settings.GetAppSupportEmailAddress());
                _Emailer.SetFromAddress(_Settings.GetShopifyFromEmailAddress());
                var _msg = $"<h3>{model.Name} wrote</h3><br/><hr/>{msg}<br/><hr>";
                _Emailer.SetMessage(_msg, true);
                _Emailer.SetSubject($"☎ {model.Subject} | {model.ShopDomain} | {model.Name}");
                var result = await _Emailer.Send(true);
                if (result)
                    _Logger.LogInformation("Sending user support email was successful.");
                else
                    _Logger.LogInformation("Sending user support email was unsuccessful.");
                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error sending support email.{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the current user context information in HTML.
        /// </summary>
        /// <param name="user">The user in context <see cref="AppUser"/></param>
        /// <returns> HTML <c>string</c> </returns>
        private string GetCurrentContextInHtml(AppUser user)
        {
            string html = "";
            var tdStyle = "style='border: 1px solid #CCC;background: #FAFAFA; text-align: left;padding:5px'";
            var thStyle = "style='border: 1px solid #CCC;background: #F3F3F3; font-weight: bold; text-align:left;padding:5px'";
            var tableStyle = "style='border: solid 1px #DDEEEE;border-collapse: collapse;border-spacing: 0;'";

            try
            {

                if (user != null)
                {
                    _Logger.LogInformation("Valid user supplied. Building html context WITH user inforamtion.");
                    html = "  <p> - Message Context - </p>";
                    html += $"<table {tableStyle}>";
                    html += $"<tr><th {thStyle}>Item</th><th {thStyle}>Value</th></tr>";
                    html += $"<tr><td {tdStyle}>User Id</td><td {tdStyle}>{user.Id}</td></tr>";
                    html += $"<tr><td {tdStyle}>User Name</td><td {tdStyle}>{user.UserName}</td></tr>";
                    html += $"<tr><td {tdStyle}>Store Email</td><td {tdStyle}>{user.Email}</td></tr>";
                    html += $"<tr><td {tdStyle}>Plan Id</td><td {tdStyle}>{user.PlanId}</td></tr>";
                    if(user.PlanId.HasValue)
                    html += $"<tr><td {tdStyle}>Plan Name</td><td {tdStyle}>{_PlanHelper[user.PlanId.Value]?.Name}</td></tr>";
                    html += $"<tr><td {tdStyle}>My Shopify Domain</td><td {tdStyle}>{user.MyShopifyDomain}</td></tr>";
                    html += $"<tr><td {tdStyle}>Billied On</td><td {tdStyle}>{user.BillingOn?.ToLongDateString()}</td></tr>";
                    html += $"<tr><td {tdStyle}>Charge Id</td><td {tdStyle}>{user.ShopifyChargeId}</td></tr>";
                    html += $"</table>";
                    _Logger.LogInformation("Done building html context.");
                }
                else
                {
                    html = $@"<p> - Message Context - </p>                            
                            <p style='color:red'>Not Available</p>                            
                            <p>&nbsp;</p>";
                    _Logger.LogWarning("User is null. Building html context WITHOUT user information.");

                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Error occurred while creating email html context.{ex.Message}");
                html = $"<p style='color:red'>Error occurred while creating email html context.<br/>{ex.Message}</p>";
            }
            return html;
        }
    }
}
