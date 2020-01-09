using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Responses;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public class Terminal
    {
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private Func<Task<string>> _selectedTextCallback;
        private bool _closingFromUi;
        private bool _exited;
        private readonly bool _requireShellProcessStart;
        private string _fallbackTitle;

        public Terminal(ITrayProcessCommunicationService trayProcessCommunicationService, byte? terminalId = null)
        {
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _trayProcessCommunicationService.TerminalExited += OnTerminalExited;
            Id = terminalId ?? trayProcessCommunicationService.GetNextTerminalId();
            _requireShellProcessStart = !terminalId.HasValue;
        }

        private void OnTerminalExited(object sender, TerminalExitStatus status)
        {
            if (status.TerminalId != Id)
            {
                return;
            }

            _exited = true;
            Exited?.Invoke(this, status.ExitCode);

            if (_closingFromUi || status.ExitCode <= 0)
            {
                Closed?.Invoke(this, System.EventArgs.Empty);
            }

            _trayProcessCommunicationService.TerminalExited -= OnTerminalExited;
        }

        public event EventHandler<int> Exited;

        /// <summary>
        /// To be observed by both view and viewmodel
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// to be observed by viewmodel
        /// </summary>
        public event EventHandler<string> KeyboardCommandReceived;

        /// <summary>
        /// to be observed by view
        /// </summary>
        public event EventHandler<byte[]> OutputReceived;

        /// <summary>
        /// to be observed by viewmodel
        /// </summary>
        public event EventHandler<TerminalSize> SizeChanged;

        /// <summary>
        /// to be observed by viewmodel
        /// </summary>
        public event EventHandler<string> TitleChanged;

        public byte Id { get; }

        /// <summary>
        /// To be called by either view or viewmodel
        /// </summary>
        public Task CloseAsync()
        {
            if (_exited)
            {
                Closed?.Invoke(this, System.EventArgs.Empty);
                return Task.CompletedTask;
            }
            _closingFromUi = true;
            _trayProcessCommunicationService.UnsubscribeFromTerminalOutput(Id);
            return _trayProcessCommunicationService.CloseTerminalAsync(Id);
        }

        /// <summary>
        /// to be called by viewmodel
        /// </summary>
        /// <returns></returns>
        public Task<string> GetSelectedText()
        {
            return _selectedTextCallback?.Invoke();
        }

        /// <summary>
        /// to be called by view
        /// </summary>
        public void ProcessKeyboardCommand(string command)
        {
            KeyboardCommandReceived?.Invoke(this, command);
        }

        /// <summary>
        /// To be set by view
        /// </summary>
        public void RegisterSelectedTextCallback(Func<Task<string>> selectedTextCallback)
        {
            _selectedTextCallback = selectedTextCallback;
        }

        /// <summary>
        /// To be called by view
        /// </summary>
        public async Task SetSizeAsync(TerminalSize size)
        {
            await _trayProcessCommunicationService.ResizeTerminalAsync(Id, size).ConfigureAwait(false);
            SizeChanged?.Invoke(this, size);
        }

        /// <summary>
        /// to be called by view
        /// </summary>
        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                title = _fallbackTitle;
            }
            TitleChanged?.Invoke(this, title);
        }

        /// <summary>
        /// To be called by view when ready
        /// </summary>
        public async Task<TerminalResponse> StartShellProcessAsync(ShellProfile shellProfile, TerminalSize size, SessionType sessionType, string termState)
        {
            if (!_requireShellProcessStart && !string.IsNullOrEmpty(termState))
            {
                OutputReceived?.Invoke(this, Encoding.UTF8.GetBytes(termState));
            }

            _trayProcessCommunicationService.SubscribeForTerminalOutput(Id, t => OutputReceived?.Invoke(this, t));

            if (_requireShellProcessStart)
            {
                var response = await _trayProcessCommunicationService
                    .CreateTerminalAsync(Id, size, shellProfile, sessionType).ConfigureAwait(false);

                if (response.Success)
                {
                    _fallbackTitle = response.ShellExecutableName;
                    SetTitle(_fallbackTitle);
                }
                return response;
            }
            else
            {
                return await _trayProcessCommunicationService.PauseTerminalOutputAsync(Id, false);
            }
        }

        public Task Write(byte[] data)
        {
            return _trayProcessCommunicationService.WriteAsync(Id, data);
        }

        /// <summary>
        /// Tells the ViewModel that this view failed to launch and can be closed.
        /// </summary>
        public void ReportLauchFailed()
        {
            Closed?.Invoke(this, System.EventArgs.Empty);
        }
    }
}