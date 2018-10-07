using Exico.Shopify.Web.Core.Plugins.Email;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SendEmail : IXCommand
    {
        public string GetName()
        {
            return "send-email";
        }

        public string GetDescription()
        {
            return "Send an email to a specific store.Supports HTML.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    xc.AskForInput(this, "Enter TO email address: ");
                    var to = Console.ReadLine();
                    if(IsValidEmail(to))
                    {
                        xc.AskForInput(this, "Enter FROM email address: ");
                        var from = Console.ReadLine();
                        if (IsValidEmail(from))
                        {
                            xc.AskForInput(this, "Enter subject: ");
                            var subject = Console.ReadLine();
                            xc.AskForInput(this, "Enter message: ");
                            var message = Console.ReadLine();
                            var emailer = scope.ServiceProvider.GetService<IEmailer>();
                            emailer.AddTo(to);
                            emailer.SetFromAddress(from);
                            emailer.SetSubject(subject);
                            emailer.SetMessage(message, true);
                            var result = await emailer.Send(true);
                            if (result) xc.WriteSuccess(this, $"Successfully sent the email to {to}.");
                            else xc.WriteError(this, $"Could not send the email to {to}.");
                        }
                        else
                        {
                            xc.WriteWarning(this, "FROM email address is not valid.");
                        }
                    }
                    else
                    {
                        xc.WriteWarning(this, "TO email address is not valid.");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                xc.WriteException(this, ex);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
