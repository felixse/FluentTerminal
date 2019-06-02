using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System;
using FluentTerminal.Models.Responses;
using FluentTerminal.App.Services;

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

        private void _terminalsManager_TerminalExited(object sender, TerminalExitStatus status)
        {
            var request = new TerminalExitedRequest(status);
            _appServiceConnection?.SendMessageAsync(CreateMessage(request));
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

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageType = (string)args.Request.Message[MessageKeys.Type];
            var messageContent = (string)args.Request.Message[MessageKeys.Content];

            if (messageType == nameof(CreateTerminalRequest))
            {
                var deferral = args.GetDeferral();

                var request = JsonConvert.DeserializeObject<CreateTerminalRequest>(messageContent);

                Logger.Instance.Debug("Received CreateTerminalRequest: {@request}", request);

                var response = _terminalsManager.CreateTerminal(request);

                Logger.Instance.Debug("Sending CreateTerminalResponse: {@response}", response);

                await args.Request.SendResponseAsync(CreateMessage(response));

                deferral.Complete();
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
            else if (messageType == nameof(WriteDataRequest))
            {
                var request = JsonConvert.DeserializeObject<WriteDataRequest>(messageContent);
                _terminalsManager.Write(request.TerminalId, request.Data);
            }
            else if (messageType == nameof(TerminalExitedRequest))
            {
                var request = JsonConvert.DeserializeObject<TerminalExitedRequest>(messageContent);
                _terminalsManager.CloseTerminal(request.TerminalId);
            }
            else if (messageType == nameof(GetAvailablePortRequest))
            {
                var deferral = args.GetDeferral();

                var response = new GetAvailablePortResponse { Port = Utilities.GetAvailablePort().Value };

                await args.Request.SendResponseAsync(CreateMessage(response));

                deferral.Complete();
            }
            else if (messageType == nameof(GetUserNameRequest))
            {
                var deferral = args.GetDeferral();

                var response = new GetUserNameResponse { UserName = Environment.UserName };

                await args.Request.SendResponseAsync(CreateMessage(response));

                deferral.Complete();
            }
            else if (messageType == nameof(SaveTextFileRequest))
            {
                var deferral = args.GetDeferral();

                SaveTextFileRequest request = JsonConvert.DeserializeObject<SaveTextFileRequest>(messageContent);

                CommonResponse response = new CommonResponse();

                try
                {
                    Utilities.SaveFile(request.Path, request.Content);

                    response.Success = true;
                }
                catch (Exception e)
                {
                    response.Success = false;
                    response.Error = e.Message;
                }

                await args.Request.SendResponseAsync(CreateMessage(response));

                deferral.Complete();
            }
            else if (messageType == nameof(GetMoshSshExecutablePathRequest))
            {
                var deferral = args.GetDeferral();

                GetMoshSshExecutablePathRequest request = JsonConvert.DeserializeObject<GetMoshSshExecutablePathRequest>(messageContent);

                GetMoshSshExecutablePathResponse response;

                try
                {
                    response = request.GetResponse();
                }
                catch (Exception e)
                {
                    response = new GetMoshSshExecutablePathResponse{Error = e.Message};
                }

                await args.Request.SendResponseAsync(CreateMessage(response));

                deferral.Complete();
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