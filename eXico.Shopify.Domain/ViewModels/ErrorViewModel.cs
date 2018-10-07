using System;

namespace Exico.Shopify.Data.Domain.ViewModels
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool RequestIdAvailable => !string.IsNullOrEmpty(RequestId);

        public bool ShowExceptionDetails { get; set; } = false;

        public Exception Exception { get; set; }

        /// <summary>
        /// Usually it is the Exeception.Message.
        /// Something that gives a summary but not the whole thing
        /// </summary>
        public string GeneralMessage { get; set; }

        /// <summary>
        /// Some sort of comfortingmessage to the users
        /// </summary>
        public string HelpMessage { get; set; }
        /// <summary>
        /// A link href only, that will take the user away from the error
        /// </summary>
        public string HelpLinkHref { get; set; }
        /// <summary>
        /// To report theis error
        /// </summary>
        public string SupportEmail { get; set; }

    }
}