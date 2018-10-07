using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Microsoft.AspNetCore.Mvc;

namespace Exico.Shopify.Web.Core.Plugins.WebMsg
{
    public interface IWebMessenger
    {
        /// <summary>
        /// Adds a danger/error type message in the <c>Temp</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempDanger(Controller controller, object msg, bool autoHide = true,  bool closable = true);
        /// <summary>
        /// Adds an information type message in the <c>Temp</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>

        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempInfo(Controller controller, object msg, bool autoHide = true,   bool closable = true);

        /// <summary>
        /// Add a  message in the <c>Temp</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="data">The message model data.</param>
        void AddTempMessage(Controller controller, WebMsgModel data);
        /// <summary>
        /// Adds a successful type message in the <c>Temp</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempSuccess(Controller controller, object msg, bool autoHide = true,   bool closable = true);
        /// <summary>
        /// Adds a warning type message in the <c>Temp</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param> 
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempWarning(Controller controller, object msg, bool autoHide = true,   bool closable = true);
        /// <summary>
        /// Adds a danger/error type message in the <c>ViewData</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param> 
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebDanger(Controller controller, object msg, bool autoHide = true,  bool closable = true);
        /// <summary>
        /// Adds an information type message in the <c>ViewData</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param> 
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebInfo(Controller controller, object msg, bool autoHide = true,   bool closable = true);
        /// <summary>
        /// Add a  message in the <c>ViewData</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="data">The message model data.</param>
        void AddWebMessage(Controller controller, WebMsgModel data);
        /// <summary>
        /// Adds a success type message in the <c>ViewData</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebSuccess(Controller controller, object msg, bool autoHide = true,   bool closable = true);
        /// <summary>
        /// Adds a warning type message in the <c>ViewData</c>.
        /// Message is displayed embeded on the page.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebWarning(Controller controller, object msg, bool autoHide = true,   bool closable = true);

        /// <summary>
        /// Gets the key for the <c>Temp</c> or <c>ViewData</c> for storing the temp or web type message.
        /// </summary>
        /// <returns>string key</returns>
        string GetKey();

        #region pop ups

        /// <summary>
        /// Adds a danger/error type message in the <c>Temp</c>
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempDangerPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);
        /// <summary>
        /// Adds an information type message in the <c>Temp</c>.
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempInfoPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);

        /// <summary>
        /// Adds a successful type message in the <c>Temp</c>.
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempSuccessPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);
        /// <summary>
        /// Adds a warning type message in the <c>Temp</c>.
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddTempWarningPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);
        /// <summary>
        /// Adds a danger/error type message in the <c>ViewData</c>.
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebDangerPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);
        /// <summary>
        /// Adds an information type message in the <c>ViewData</c>
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebInfoPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);

        /// <summary>
        /// Adds a success type message in the <c>ViewData</c>
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebSuccessPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);
        /// <summary>
        /// Adds a warning type message in the <c>ViewData</c>.
        /// Message is displayed in a pop-up dialog.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="msg">The message.</param>
        /// <param name="autoHide">if set to <c>true</c> then automatically hide after interval.</param>        
        /// <param name="closable">if set to <c>true</c> then it is closable.</param>
        void AddWebWarningPopUp(Controller controller, object msg, bool autoHide = true, bool closable = true);

        #endregion
    }
}