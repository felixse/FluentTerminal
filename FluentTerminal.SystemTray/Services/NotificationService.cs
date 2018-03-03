using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace FluentTerminal.SystemTray.Services
{
    public class NotificationService
    {
        public void ShowNotification(string title, string content)
        {
            string xml = $@"<toast>
                            <visual>
                                <binding template='ToastGeneric'>
                                    <text>{title}</text>
                                    <text>{content}</text>
                                </binding>
                            </visual>
                        </toast>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var toast = new ToastNotification(doc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
