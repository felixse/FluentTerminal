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

            _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        private void _terminalsManager_DisplayOutputRequested(object sender, DisplayTerminalOutputRequest e)
        {
            _appServiceConnection.SendMessageAsync(CreateMessage(e));
        }

        public void StartAppServiceConnection()
        {
            _appServiceConnection = new AppServiceConnection
            {
                AppServiceName = "FluentTerminalAppService",
                PackageFamilyName = Package.Current.Id.FamilyName
            };
            _appServiceConnection.RequestReceived += OnRequestReceived;
            _appServiceConnection.ServiceClosed += OnServiceClosed;

            _appServiceConnection.OpenAsync();
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _appServiceConnection.RequestReceived -= OnRequestReceived;
            _appServiceConnection.ServiceClosed -= OnServiceClosed;

            _appServiceConnection = null;
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageType = (string)args.Request.Message[MessageKeys.Type];
            var messageContent = (string)args.Request.Message[MessageKeys.Content];

            if (messageType == nameof(CreateTerminalRequest))
            {
                var request = JsonConvert.DeserializeObject<CreateTerminalRequest>(messageContent);
                var response = _terminalsManager.CreateTerminal(request);

                args.Request.SendResponseAsync(CreateMessage(response));
            }
            else if (messageType == nameof(ResizeTerminalRequest))
            {
                var request = JsonConvert.DeserializeObject<ResizeTerminalRequest>(messageContent);

                _terminalsManager.ResizeTerminal(request.TerminalId, request.NewSize);
            }
            else if (messageType == nameof(SetToggleWindowKeyBindingsRequest))
            {
                var request = JsonConvert.DeserializeObject<SetToggleWindowKeyBindingsRequest>(messageContent);

                _toggleWindowService.SetHotKeys(request.KeyBindings);
            }
            else if (messageType == nameof(WriteTextRequest))
            {
                var request = JsonConvert.DeserializeObject<WriteTextRequest>(messageContent);
                _terminalsManager.WriteText(request.TerminalId, request.Text);
            }
            else if (messageType == nameof(TerminalExitedRequest))
            {
                var request = JsonConvert.DeserializeObject<TerminalExitedRequest>(messageContent);
                _terminalsManager.CloseTerminal(request.TerminalId);
            }
        }

        private ValueSet CreateMessage(object content)
        {
            return new ValueSet
            {
                [MessageKeys.Type] = content.GetType().Name,
                [MessageKeys.Content] = JsonConvert.SerializeObject(content)
            };
        }
    }
}