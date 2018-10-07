using System;

namespace Exico.Shopify.Web.Core.Plugins.WebMsg
{
    /// <summary>
    /// This class is used to store the message for the front end. 
    /// It is used in rendering the web message view as well.
    /// </summary>
    [Serializable]
    public class WebMsgModel
    {
        public WebMsgModel() { }
        public WebMsgModel(object msgObject, WEB_MSG_TYPE type)
        {
            MsgData = msgObject;
            MsgType = type;
            UniqueId = Guid.NewGuid().ToString();
        }

        public WebMsgModel(object msgObject, WEB_MSG_TYPE type, bool autoHide, bool popUp, bool closable) : this(msgObject, type)
        {
            IsAutoHide = autoHide;
            IsClosable = closable;
            IsPopUp = popUp;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the message box should automatically hide.
        /// Default value is <c>true</c>
        /// </summary>
        /// <value>
        ///   <c>true</c> if if it is automatic hide; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoHide { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether the message is pop up.
        /// Default value is <c>false</c>
        /// </summary>
        /// <value>
        ///   <c>true</c> if message should pop up; otherwise, <c>false</c>.
        /// </value>
        public bool IsPopUp { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is closable.
        /// Default value is <c>true</c>
        /// </summary>
        /// <value>
        ///   <c>true</c> if closable; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosable { get; set; } = true;
        /// <summary>
        /// Gets or sets the type of the message.
        /// <see cref="WEB_MSG_TYPE"/>
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public WEB_MSG_TYPE MsgType { get; set; }

        /// <summary>
        /// Gets or sets the message data.
        /// It is usually string value. But you can 
        /// assign object and then retieve an object during the 
        /// view rendering to display complex message.
        /// </summary>
        /// <value>
        /// The MSG data.
        /// </value>
        public Object MsgData { get; set; }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <returns>A unique id</returns>
        public string UniqueId { get; set; }

    }
}