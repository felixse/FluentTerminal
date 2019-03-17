using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Implementation
{
    public class TrayProcessCommunicationService : ITrayProcessCommunicationService
    {
        private readonly ISettingsService _settingsService;
        private IAppServiceConnection _appServiceConnection;
        private readonly Dictionary<int, Action<byte[]>> _terminalOutputHandlers;
        private int _nextTerminalId = 0;

        public event EventHandler<int> TerminalExited;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _terminalOutputHandlers = new Dictionary<int, Action<byte[]>>();
        }

        public int GetNextTerminalId()
        {
            return _nextTerminalId++;
        }

        public async Task<GetAvailablePortResponse> GetAvailablePort()
        {
            var request = new GetAvailablePortRequest();

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateSerializedMessage(request));
            var response = MessagePackSerializer.Deserialize<GetAvailablePortResponse>(responseMessage.Data);

            Logger.Instance.Debug("Received GetAvailablePortResponse: {@response}", response);

            return response;
        }

        public async Task<CreateTerminalResponse> CreateTerminal(int id, TerminalSize size, ShellProfile shellProfile, SessionType sessionType)
        {
            var request = new CreateTerminalRequest
            {
                Id = id,
                Size = size,
                Profile = shellProfile,
                SessionType = sessionType
            };

            Logger.Instance.Debug("Sending CreateTerminalRequest: {@request}", request);

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateSerializedMessage(request));
            var response = MessagePackSerializer.Deserialize<CreateTerminalResponse>(responseMessage.Data);

            Logger.Instance.Debug("Received CreateTerminalResponse: {@response}", response);

            return response;
        }

        public void Initialize(IAppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            _appServiceConnection.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, SerializedMessage e)
        {
            if (e.Identifier == DisplayTerminalOutputRequest.Identifier)
            {
                var request = MessagePackSerializer.Deserialize<DisplayTerminalOutputRequest>(e.Data);

                if (_terminalOutputHandlers.ContainsKey(request.TerminalId))
                {
                    _terminalOutputHandlers[request.TerminalId].Invoke(request.Output);
                }
                else
                {
                    Logger.Instance.Error("Received output for unknown terminal Id {id}", request.TerminalId);
                }
            }
            else if (e.Identifier == TerminalExitedRequest.Identifier)
            {
                var request = MessagePackSerializer.Deserialize<TerminalExitedRequest>(e.Data);

                Logger.Instance.Debug("Received TerminalExitedRequest: {@request}", request);

                TerminalExited?.Invoke(this, request.TerminalId);
            }
        }

        public Task ResizeTerminal(int id, TerminalSize size)
        {
            var request = new ResizeTerminalRequest
            {
                TerminalId = id,
                NewSize = size
            };

            return _appServiceConnection.SendMessageAsync(CreateSerializedMessage(request));
        }

        public void SubscribeForTerminalOutput(int terminalId, Action<byte[]> callback)
        {
            _terminalOutputHandlers[terminalId] = callback;
        }

        public Task UpdateToggleWindowKeyBindings()
        {
            var keyBindings = _settingsService.GetCommandKeyBindings()[nameof(Command.ToggleWindow)];

            var request = new SetToggleWindowKeyBindingsRequest
            {
                KeyBindings = keyBindings
            };

            return _appServiceConnection.SendMessageAsync(CreateSerializedMessage(request));
        }

        public Task Write(int terminalId, byte[] data)
        {
            var request = new WriteDataRequest
            {
                TerminalId = terminalId,
                Data = data
            };

            return _appServiceConnection.SendMessageAsync(CreateSerializedMessage(request));
        }

        public Task CloseTerminal(int terminalId)
        {
            var request = new TerminalExitedRequest
            {
                TerminalId = terminalId
            };

            Logger.Instance.Debug("Sending TerminalExitedRequest: {@request}", request);

            return _appServiceConnection.SendMessageAsync(CreateSerializedMessage(request));
        }

        private SerializedMessage CreateSerializedMessage<T>(T message) where T : IMessage
        {
            var identifier = message.Identifier;
            var data = MessagePackSerializer.Serialize(message);

            return new SerializedMessage(identifier, data);
        }
    }
}