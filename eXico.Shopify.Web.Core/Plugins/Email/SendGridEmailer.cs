using Exico.Shopify.Web.Core.Modules;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Plugins.Email
{
    //TODO add deleting or clearning to,from,bcc,attachment etc
    /// <summary>
    /// Uses sendgrid web api to send emails.
    /// You need to register with sendgrid , for testing get a gree API key from them
    /// https://sendgrid.com/free/
    /// "Send up to 40,000 emails for 30 days, then send 100 emails/day free forever"
    /// </summary>
    /// <seealso cref="Exico.Shopify.Web.Core.Plugins.Email.IEmailer" />
    public class SendGridEmailer : IEmailer
    {
        private readonly string ApiKey;
        private SendGridClient client;
        private SendGridMessage message;
        private readonly ILogger<SendGridEmailer> logger;


        public SendGridEmailer(IDbSettingsReader settings, ILogger<SendGridEmailer> logger)
        {
            ApiKey = settings.GetGetSendGridApiKey();
            client = new SendGridClient(ApiKey);
            message = new SendGridMessage();
            this.logger = logger;
        }

        /// <summary>
        /// Adds the attachment using bytes content.
        /// </summary>
        /// <param name="bytes">The bytes  value of the source file.</param>
        /// <param name="attachmentName">Name of the atatchment.</param>
        public void AddAttachment(byte[] bytes, string attachmentName)
        {
            logger.LogInformation($"Adding bytes attachment. Size {bytes.Length} and attachment name is {attachmentName}.");
            var Content = Convert.ToBase64String(bytes);
            message.AddAttachment(attachmentName, Content);
            logger.LogInformation("Done adding bytes attachment.");
        }

        /// <summary>
        /// Adds the attachment using source file path
        /// </summary>
        /// <param name="sourceFilePath">The file path.</param>
        public void AddAttachment(string sourceFilePath)
        {
            logger.LogInformation($"Adding file attachment.File name is {sourceFilePath}.");
            var bytes = File.ReadAllBytes(sourceFilePath);
            var fileName = Path.GetFileName(sourceFilePath);
            AddAttachment(bytes, fileName);
            logger.LogInformation($"Done adding bytes attachment. Attached as {fileName}.");
        }

        /// <summary>
        /// Adds the BCC.
        /// Takes a list of emails and add them as bcc email address one by one.
        /// </summary>
        /// <param name="emails">The emails.</param>
        public void AddBcc(List<string> emails)
        {
            logger.LogInformation("Adding list of bcc emails.");
            foreach (var e in emails)
            {
                AddBcc(e);
            }
            logger.LogInformation($"Done adding Total {emails.Count} as bcc.");
        }

        /// <summary>
        /// Adds the BCC.
        /// Takes a single emails address and adds that as a bcc email address.
        /// </summary>
        /// <param name="email">The email.</param>
        public void AddBcc(string email)
        {
            logger.LogInformation("Adding single bcc.");
            message.AddBcc(new EmailAddress(email));
            logger.LogInformation($"Done adding {email} as bcc.");
        }

        /// <summary>
        /// Adds the CC.
        /// Takes a list of emails and add them as cc email address one by one.
        /// </summary>
        /// <param name="emails">The emails.</param>
        public void AddCc(List<string> emails)
        {
            logger.LogInformation("Adding list of cc emails.");
            foreach (var e in emails)
            {
                AddCc(e);
            }
            logger.LogInformation($"Done adding Total {emails.Count} as cc.");
        }

        /// <summary>
        /// Adds the CC.
        /// Takes a single emails address and adds that as a cc email address.
        /// </summary>
        /// <param name="email">The email.</param>
        public void AddCc(string email)
        {
            logger.LogInformation("Adding single cc.");
            message.AddCc(new EmailAddress(email));
            logger.LogInformation($"Done adding {email} as cc.");
        }

        /// <summary>
        /// Adds the TO address.
        /// Takes a list of emails and add them as TO email address one by one.
        /// </summary>
        /// <param name="emails">The emails.</param>
        public void AddTo(List<string> emails)
        {
            logger.LogInformation("Adding list of to emails.");
            foreach (var e in emails)
            {
                AddTo(e);
            }
            logger.LogInformation($"Done adding Total {emails.Count} to email.");
        }

        /// <summary>
        /// Adds the CC.
        /// Takes a single emails address and adds that as a TO email address.
        /// </summary>
        /// <param name="email">The email.</param>
        public void AddTo(string email)
        {
            logger.LogInformation("Adding single cc.");
            message.AddTo(new EmailAddress(email));
            logger.LogInformation($"Done adding {email} as to email.");
        }

        /// <summary>
        /// Starts sending the email using SendGrid api.
        /// </summary>
        /// <param name="autoResset">if set to <c>true</c> then after sending it reinitiates the 
        /// bcc,cc,to and from email address as well as well resets the message and the subject.
        /// </param>
        /// <returns>true or false</returns>
        public async Task<bool> Send(bool autoResset = false)
        {
            Exception e = null;
            bool ret = false;
            try
            {
                logger.LogInformation("Trying to send email via sendgrid...");
                var response = await client.SendEmailAsync(message);
                ret = (response.StatusCode == System.Net.HttpStatusCode.OK) || (response.StatusCode == System.Net.HttpStatusCode.Accepted);
                if (!ret)
                {
                    logger.LogError("Sending email failed.");
                    logger.LogError($"Server Responded with { response.StatusCode}.");
                }
                else
                {
                    logger.LogInformation($"Done sending email. Server resonded with code {response.StatusCode}.");
                }

                return ret;
            }
            catch (Exception ex)
            {
                e = new Exception("Could not send email using sendgrid.", ex);
                logger.LogError(e, "Error sending email.");
            }
            finally
            {
                if (autoResset)
                {
                    logger.LogInformation("Auto reseting as requested.");
                    Resset();
                }
            }

            return ret;
        }

        /// <summary>
        /// Sets the email message.
        /// </summary>
        /// <param name="msg">The message body.</param>
        /// <param name="isHtml">if set to <c>true</c> then the email is sent as html email otherwise as text.</param>
        public void SetMessage(string msg, bool isHtml)
        {
            logger.LogInformation($"Setting email message. \"{msg}\"");
            if (isHtml)
            {
                message.AddContent("text/html", msg);
                logger.LogInformation("Message is set as text/html.");
            }
            else
            {
                message.AddContent("text/plain", msg);
                logger.LogInformation("Message is set as text/plain.");
            }
        }

        /// <summary>
        /// Sets the subject for the email.
        /// </summary>
        /// <param name="subject">The subject.</param>
        public void SetSubject(string subject)
        {
            logger.LogInformation($"Setting email subject to {subject}");
            message.SetSubject(subject);
            logger.LogInformation($"Setting email subject done.");
        }

        /// <summary>
        /// Sets from email address.
        /// </summary>
        /// <param name="email">The email address .</param>
        /// <param name="name">And the name.</param>
        public void SetFromAddress(string email, string name = "")
        {
            logger.LogInformation($"Setting from email address to email {email} and name {name}");
            message.SetFrom(new EmailAddress(email, name));
            logger.LogInformation($"Setting from email address done.");
        }

        /// <summary>
        /// Reinitiates the emailer.
        /// So that you can start building a new email address.
        /// </summary>
        public void Resset()
        {
            logger.LogInformation("Resetting 'SendGridMessage' object.");
            message = new SendGridMessage();
            logger.LogInformation("Resetting done.");
        }
    }
}
