using System.Diagnostics;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace FluentTerminal.App.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        public void ShowNotification(string title, string content, string url = null)
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
            if (url != null)
            {
                toast.Activated += (n, o) => Process.Start(url);
            }
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}