using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Plugins.Email
{

    /// <summary>
    /// Interface for implementing email service. 
    /// The default implementation is SendGridEmailer.
    /// 
    /// </summary>
    public interface IEmailer
    {
        Task<bool> Send(bool autoResset = false);
        void SetFromAddress(string email, string name = "");
        void AddTo(string email);
        void AddTo(List<string> emails);
        void AddBcc(string email);
        void AddBcc(List<string> emails);
        void AddCc(string email);
        void AddCc(List<string> emails);
        void AddAttachment(string filePath);
        void AddAttachment(byte[] bytes, string filename = "");
        void SetMessage(string msg, Boolean isHtml);
        void SetSubject(string subject);
        void Resset();

    }

}
