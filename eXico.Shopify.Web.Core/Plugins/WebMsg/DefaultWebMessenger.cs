using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Web;

namespace Exico.Shopify.Web.Core.Plugins.WebMsg
{

    /// <summary>
    /// To understand this default implementation please refer to <see cref="IWebMessenger"/>.
    /// This class is registered as default <c>IWebMessenger</c> service in the <see cref="Extensions.AppBuilderExtensions"/>
    /// </summary>
    /// <seealso cref="Exico.Shopify.Web.Core.Plugins.WebMsg.IWebMessenger" />
    public class DefaultWebMessenger : IWebMessenger
    {
        //TODO Another set of methods for pop up type
        private string _Key = "";
        public DefaultWebMessenger(IWebMsgConfig config)
        {
            _Key = config.Key;
        }
        public string GetKey() => _Key;

        #region Msg Helper Methods

        #region Temp Msgs
        private void AddTempMsgs(Controller controller, WEB_MSG_TYPE type, object msg, bool autoHide = true, bool popUp = false, bool closable = true)
        {
            var seriaLizedMsg= JsonConvert.SerializeObject(new WebMsgModel(msg, type, autoHide, popUp, closable));
            controller.TempData[GetKey()] = seriaLizedMsg;
        }

        public void AddTempInfo(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
           AddTempMsgs(controller, WEB_MSG_TYPE.INFORMATION, msg, autoHide, false, closable);
        public void AddTempDanger(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            AddTempMsgs(controller, WEB_MSG_TYPE.DANGER, msg, autoHide, false, closable);
        public void AddTempSuccess(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
           AddTempMsgs(controller, WEB_MSG_TYPE.SUCCESS, msg, autoHide, false, closable);
        public void AddTempWarning(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
           AddTempMsgs(controller, WEB_MSG_TYPE.WARNING, msg, autoHide, false, closable);

        public void AddTempInfoPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
           AddTempMsgs(controller, WEB_MSG_TYPE.INFORMATION, msg, autoHide, true, closable);
        public void AddTempDangerPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
           AddTempMsgs(controller, WEB_MSG_TYPE.DANGER, msg, autoHide, true, closable);
        public void AddTempSuccessPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
           AddTempMsgs(controller, WEB_MSG_TYPE.SUCCESS, msg, autoHide, true, closable);
        public void AddTempWarningPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
           AddTempMsgs(controller, WEB_MSG_TYPE.WARNING, msg, autoHide, true, closable);
        public void AddTempMessage(Controller controller, WebMsgModel data) =>
           controller.TempData[GetKey()] = data;
        #endregion

        #region Web Msgs
        public void AddWebInfo(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.INFORMATION, autoHide, false, closable);
        public void AddWebDanger(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.DANGER, autoHide, false, closable);
        public void AddWebSuccess(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.SUCCESS, autoHide, false, closable);
        public void AddWebWarning(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.WARNING, autoHide, false, closable);

        public void AddWebInfoPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
             controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.INFORMATION, autoHide, true, closable);
        public void AddWebDangerPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.DANGER, autoHide, true, closable);
        public void AddWebSuccessPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.SUCCESS, autoHide, true, closable);
        public void AddWebWarningPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true) =>
            controller.ViewData[GetKey()] = new WebMsgModel(msg, WEB_MSG_TYPE.WARNING, autoHide, true, closable);
        public void AddWebMessage(Controller controller, WebMsgModel data) =>
            controller.ViewData[GetKey()] = data;
        #endregion

        #endregion
    }
}