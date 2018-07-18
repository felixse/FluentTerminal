using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace FluentTerminal.SystemTray.Services
{
    public class AppCommunicationService
    {
        private AppServiceConnection _appServiceConnection;
        private readonly TerminalsManager _terminalsManager;
        private readonly ToggleWindowService _toggleWindowService;

        public static string EventWaitHandleName => "FluentTerminalNewInstanceEvent";

        public AppCommunicationService(TerminalsManager terminalsManager, ToggleWindowService toggleWindowService)
        {
            _terminalsManager = terminalsManager;
            _terminalsManager.DisplayOutputRequested += _terminalsManager_DisplayOutputRequested;
            _terminalsManager.TerminalExited += _terminalsManager_TerminalExited;
            _toggleWindowService = toggleWindowService;

            var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventWaitHandleName);

            Task.Run(() =>
            {
                while (true)
                {
                    eventWaitHandle.WaitOne();
                    StartAppServiceConnection();
                }
            });
        }

        private void _terminalsManager_TerminalExited(object sender, int e)
        {
            var request = new TerminalExitedRequest
            {
                TerminalId = e
            };

            var message = new ValueSet
            {
                { MessageKeys.Type, MessageTypes.TerminalExitedRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            _appServiceConnection.SendMessageAsync(message);
        }

        private void _terminalsManager_DisplayOutputRequested(object sender, DisplayTerminalOutputRequest e)
        {
            var message = new ValueSet
            {
                { MessageKeys.Type, MessageTypes.DisplayTerminalOutputRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(e) }
            };

            _appServiceConnection.SendMessageAsync(message);
        }

        public void StartAppServiceConnection()
        {
            if (_appServiceConnection != null)
            {
                //_notificationService.ShowNotification("Start app service", "appservice was not null");
            }

            _appServiceConnection = new AppServiceConnection
            {
                AppServiceName = "FluentTerminalAppService",
                PackageFamilyName = Package.Current.Id.FamilyName
            };
            _appServiceConnection.RequestReceived += _appServiceConnection_RequestReceived;
            _appServiceConnection.ServiceClosed += _appServiceConnection_ServiceClosed;

            _appServiceConnection.OpenAsync();
        }

        private void _appServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _appServiceConnection.RequestReceived -= _appServiceConnection_RequestReceived;
            _appServiceConnection.ServiceClosed -= _appServiceConnection_ServiceClosed;

            _appServiceConnection = null;
        }

        private void _appServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageType = (string)args.Request.Message[MessageKeys.Type];
            var messageContent = (string)args.Request.Message[MessageKeys.Content];

            if (messageType == MessageTypes.CreateTerminalRequest)
            {
                var request = JsonConvert.DeserializeObject<CreateTerminalRequest>(messageContent);
                var response = _terminalsManager.CreateTerminal(request);

                var message = new ValueSet
                {
                    { MessageKeys.Type, MessageTypes.CreateTerminalResponse },
                    { MessageKeys.Content, JsonConvert.SerializeObject(response) }
                };

                args.Request.SendResponseAsync(message);
            }
            else if (messageType == MessageTypes.ResizeTerminalRequest)
            {
                var request = JsonConvert.DeserializeObject<ResizeTerminalRequest>(messageContent);

                _terminalsManager.ResizeTerminal(request.TerminalId, request.NewSize);
            }
            else if (messageType == MessageTypes.SetToggleWindowKeyBindingsRequest)
            {
                var request = JsonConvert.DeserializeObject<SetToggleWindowKeyBindingsRequest>(messageContent);

                _toggleWindowService.SetHotKeys(request.KeyBindings);
            }
            else if (messageType == MessageTypes.WriteTextRequest)
            {
                var request = JsonConvert.DeserializeObject<WriteTextRequest>(messageContent);
                _terminalsManager.WriteText(request.TerminalId, request.Text);
            }
            else if (messageType == MessageTypes.TerminalExitedRequest)
            {
                var request = JsonConvert.DeserializeObject<TerminalExitedRequest>(messageContent);
                _terminalsManager.CloseTerminal(request.TerminalId);
            }
        }
    }
}