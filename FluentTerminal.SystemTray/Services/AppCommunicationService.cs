using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System;
using FluentTerminal.Models.Responses;
using FluentTerminal.App.Services;
using MessagePack;

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

            _appServiceConnection?.SendMessageAsync(CreateSerializedMessage(request));
        }

        private void _terminalsManager_DisplayOutputRequested(object sender, DisplayTerminalOutputRequest e)
        {
            _appServiceConnection.SendMessageAsync(CreateSerializedMessage(e));
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

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageType = (byte)args.Request.Message[MessageKeys.Type];
            var messageContent = (byte[])args.Request.Message[MessageKeys.Content];

            if (messageType == CreateTerminalRequest.Identifier)
            {
                var deferral = args.GetDeferral();

                var request = MessagePackSerializer.Deserialize<CreateTerminalRequest>(messageContent);

                Logger.Instance.Debug("Received CreateTerminalRequest: {@request}", request);

                var response = _terminalsManager.CreateTerminal(request);

                Logger.Instance.Debug("Sending CreateTerminalResponse: {@response}", response);

                await args.Request.SendResponseAsync(CreateSerializedMessage(response));

                deferral.Complete();
            }
            else if (messageType == ResizeTerminalRequest.Identifier)
            {
                var request = MessagePackSerializer.Deserialize<ResizeTerminalRequest>(messageContent);

                _terminalsManager.ResizeTerminal(request.TerminalId, request.NewSize);
            }
            else if (messageType == SetToggleWindowKeyBindingsRequest.Identifier)
            {
                var request = MessagePackSerializer.Deserialize<SetToggleWindowKeyBindingsRequest>(messageContent);

                _toggleWindowService.SetHotKeys(request.KeyBindings);
            }
            else if (messageType == WriteDataRequest.Identifier)
            {
                var request = MessagePackSerializer.Deserialize<WriteDataRequest>(messageContent);
                _terminalsManager.Write(request.TerminalId, request.Data);
            }
            else if (messageType == TerminalExitedRequest.Identifier)
            {
                var request = MessagePackSerializer.Deserialize<TerminalExitedRequest>(messageContent);
                _terminalsManager.CloseTerminal(request.TerminalId);
            }
            else if (messageType == GetAvailablePortRequest.Identifier)
            {
                var deferral = args.GetDeferral();

                var response = new GetAvailablePortResponse { Port = Utilities.GetAvailablePort().Value };

                await args.Request.SendResponseAsync(CreateSerializedMessage(response));

                deferral.Complete();
            }
        }

        private ValueSet CreateSerializedMessage<T>(T message) where T : IMessage
        {
            return new ValueSet
            {
                [MessageKeys.Type] = message.Identifier,
                [MessageKeys.Content] = MessagePackSerializer.Serialize(message)
            };
        }
    }
}