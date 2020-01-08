﻿using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System;
using System.IO;
using System.Linq;
using FluentTerminal.Models.Responses;
using FluentTerminal.App.Services;
using System.Windows.Forms;
using Windows.Foundation;

namespace FluentTerminal.SystemTray.Services
{
    public class AppCommunicationService
    {
        private AppServiceConnection _appServiceConnection;
        private readonly TerminalsManager _terminalsManager;
        private readonly ToggleWindowService _toggleWindowService;
        private readonly ISettingsService _settingsService;

        public const string EventWaitHandleName = "FluentTerminalNewInstanceEvent";

        public AppCommunicationService(TerminalsManager terminalsManager, ToggleWindowService toggleWindowService, ISettingsService settingsService)
        {
            _terminalsManager = terminalsManager;
            _terminalsManager.DisplayOutputRequested += _terminalsManager_DisplayOutputRequested;
            _terminalsManager.TerminalExited += _terminalsManager_TerminalExited;
            _toggleWindowService = toggleWindowService;
            _settingsService = settingsService;

            Task.Factory.StartNew(() =>
            {
                var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventWaitHandleName);

                while (true)
                {
                    eventWaitHandle.WaitOne();
                    // ReSharper disable once AssignmentIsFullyDiscarded
                    _ = StartAppServiceConnectionAsync();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void _terminalsManager_TerminalExited(object sender, TerminalExitStatus status)
        {
            var request = new TerminalExitedRequest(status);
            _appServiceConnection?.SendMessageAsync(CreateMessage(request));
        }

        private void _terminalsManager_DisplayOutputRequested(object sender, TerminalOutput e)
        {
            var message = new ValueSet
            {
                [MessageKeys.Type] = Constants.TerminalBufferRequestIdentifier,
                [MessageKeys.TerminalId] = e.TerminalId,
                [MessageKeys.Content] = e.Data
            };

            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = _appServiceConnection.SendMessageAsync(message);
        }

        public IAsyncOperation<AppServiceConnectionStatus> StartAppServiceConnectionAsync()
        {
            _appServiceConnection = new AppServiceConnection
            {
                AppServiceName = "FluentTerminalAppService",
                PackageFamilyName = Package.Current.Id.FamilyName
            };
            _appServiceConnection.RequestReceived += OnRequestReceived;
            _appServiceConnection.ServiceClosed += OnServiceClosed;

            return _appServiceConnection.OpenAsync();
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

            switch ((MessageIdentifiers) messageType)
            {
                case MessageIdentifiers.WriteDataMessage:
                    HandleWriteDataMessage(args);
                    return;
                case MessageIdentifiers.CreateTerminalRequest:
                    await HandleCreateTerminalRequestAsync(args).ConfigureAwait(false);
                    return;
                case MessageIdentifiers.ResizeTerminalRequest:
                    HandleResizeTerminalRequest(args);
                    return;
                case MessageIdentifiers.SetToggleWindowKeyBindingsRequest:
                    HandleSetToggleWindowKeyBindingsRequest(args);
                    return;
                case MessageIdentifiers.TerminalExitedRequest:
                    HandleTerminalExitedRequest(args);
                    return;
                case MessageIdentifiers.GetUserNameRequest:
                    await HandleGetUserNameRequestAsync(args).ConfigureAwait(false);
                    return;
                case MessageIdentifiers.SaveTextFileRequest:
                    await HandleSaveTextFileRequestAsync(args).ConfigureAwait(false);
                    return;
                case MessageIdentifiers.GetSshConfigFolderRequest:
                    await HandleGetSshConfigFolderRequestAsync(args).ConfigureAwait(false);
                    return;
                case MessageIdentifiers.CheckFileExistsRequest:
                    await HandleCheckFileExistsRequestAsync(args).ConfigureAwait(false);
                    return;
                case MessageIdentifiers.MuteTerminalRequest:
                    HandleMuteTerminalRequest(args);
                    return;
                case MessageIdentifiers.UpdateSettingsRequest:
                    HandleUpdateSettingsRequest(args);
                    return;
                case MessageIdentifiers.GetCommandPathRequest:
                    await GetCommandPathRequestHandlerAsync(args).ConfigureAwait(false);
                    return;
                case MessageIdentifiers.PauseTerminalOutputRequest:
                    await HandlePauseTerminalOutputRequestAsync(args).ConfigureAwait(false);
                    return;
                case MessageIdentifiers.QuitApplicationRequest:
                    HandleQuitApplicationRequest();
                    return;
                default:
                    Logger.Instance.Error("Received unknown message type: {messageType}", messageType);
                    return;
            }
        }

        private void HandleQuitApplicationRequest()
        {
            Application.Exit();
        }

        private void HandleWriteDataMessage(AppServiceRequestReceivedEventArgs args)
        {
            var terminalId = (byte)args.Request.Message[MessageKeys.TerminalId];
            var content = (byte[])args.Request.Message[MessageKeys.Content];
            _terminalsManager.Write(terminalId, content);
        }

        private async Task HandleCreateTerminalRequestAsync(AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<CreateTerminalRequest>(messageContent);
            var response = _terminalsManager.CreateTerminal(request);
            await args.Request.SendResponseAsync(CreateMessage(response));
            deferral.Complete();
        }

        private void HandleResizeTerminalRequest(AppServiceRequestReceivedEventArgs args)
        {
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<ResizeTerminalRequest>(messageContent);
            _terminalsManager.ResizeTerminal(request.TerminalId, request.NewSize);
        }

        private void HandleSetToggleWindowKeyBindingsRequest(AppServiceRequestReceivedEventArgs args)
        {
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<SetToggleWindowKeyBindingsRequest>(messageContent);
            _toggleWindowService.SetHotKeys(request.KeyBindings);
        }

        private void HandleTerminalExitedRequest(AppServiceRequestReceivedEventArgs args)
        {
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<TerminalExitedRequest>(messageContent);
            _terminalsManager.CloseTerminal(request.TerminalId);
        }

        private async Task HandleGetUserNameRequestAsync(AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var response = new StringValueResponse { Success = !string.IsNullOrEmpty(Environment.UserName), Value = Environment.UserName };
            await args.Request.SendResponseAsync(CreateMessage(response));
            deferral.Complete();
        }

        private async Task HandleSaveTextFileRequestAsync(AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<SaveTextFileRequest>(messageContent);
            var response = new CommonResponse();

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

        private async Task HandleGetSshConfigFolderRequestAsync(AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<GetSshConfigFolderRequest>(messageContent);
            var response = new GetSshConfigFolderResponse();

            try
            {
                var sshDir = new DirectoryInfo(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh"));

                if (sshDir.Exists)
                {
                    response.Path = sshDir.FullName;

                    if (request.IncludeContent)
                    {
                        response.Files = sshDir.GetFiles().Select(fi => fi.Name).ToArray();
                    }
                }

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

        private void HandleMuteTerminalRequest(AppServiceRequestReceivedEventArgs args)
        {
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<MuteTerminalRequest>(messageContent);
            Utilities.MuteTerminal(request.Mute);
        }

        private void HandleUpdateSettingsRequest(AppServiceRequestReceivedEventArgs args)
        {
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<UpdateSettingsRequest>(messageContent);
            _settingsService.NotifyApplicationSettingsChanged(request.Settings);
        }
        
        private async Task HandlePauseTerminalOutputRequestAsync(AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<PauseTerminalOutputRequest>(messageContent);
            var response = _terminalsManager.PauseTermimal(request.Id, request.Pause);

            await args.Request.SendResponseAsync(CreateMessage(response));

            deferral.Complete();
        }

        private async Task HandleCheckFileExistsRequestAsync(AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<CheckFileExistsRequest>(messageContent);
            var response = new CommonResponse();

            try
            {
                response.Success = System.IO.File.Exists(request.Path);

                if (!response.Success)
                {
                    response.Error = "File not found.";
                }
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
            }

            await args.Request.SendResponseAsync(CreateMessage(response));

            deferral.Complete();
        }

        private async Task GetCommandPathRequestHandlerAsync(AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var messageContent = (string)args.Request.Message[MessageKeys.Content];
            var request = JsonConvert.DeserializeObject<GetCommandPathRequest>(messageContent);
            var response = new StringValueResponse();

            try
            {
                response.Value = request.Command.GetCommandPath();
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

        private ValueSet CreateMessage(IMessage content)
        {
            return new ValueSet
            {
                [MessageKeys.Type] = content.Identifier,
                [MessageKeys.Content] = JsonConvert.SerializeObject(content)
            };
        }
    }
}