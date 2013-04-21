using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ImgurUploader
{
    public class Toaster
    {
        public static ToastNotification MakeToast(string title, string body, string launch)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            IXmlNode toastNode = (toastXml.GetElementsByTagName("toast"))[0];
            IXmlNode textNode = (toastXml.GetElementsByTagName("text"))[0];
            IXmlNode bodyNode = (toastXml.GetElementsByTagName("text"))[1];

            if (launch != null)
            {
                XmlAttribute launchAttr = toastXml.CreateAttribute("launch");
                launchAttr.Value = launch;
                toastNode.Attributes.SetNamedItem(launchAttr);
            }

            textNode.AppendChild(toastXml.CreateTextNode(title));
            bodyNode.AppendChild(toastXml.CreateTextNode(body));

            ToastNotification toast = new ToastNotification(toastXml);
            return toast;
        }
    }
}
