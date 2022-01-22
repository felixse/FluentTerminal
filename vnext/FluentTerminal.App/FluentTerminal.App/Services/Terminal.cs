using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Responses;
using FluentTerminal.SystemTray.Services.ConPty;
using FluentTerminal.SystemTray.Services.WinPty;
using System;
using System.Diagnostics;
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
        private ITerminalSession _session;

        private static byte _nextTerminalId = 0;

        public static byte GetNextTerminalId()
        {
            return _nextTerminalId++;
        }

        public Terminal(byte? terminalId = null)
        {
            //_trayProcessCommunicationService = trayProcessCommunicationService;
            //_trayProcessCommunicationService.TerminalExited += OnTerminalExited;
            Id = terminalId ?? GetNextTerminalId();
            _requireShellProcessStart = !terminalId.HasValue;
        }

        public void Reconnect()
        {
            _exited = false;
            _trayProcessCommunicationService.TerminalExited += OnTerminalExited;
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

        public ShellProfile Profile { get; private set; }

        /// <summary>
        /// To be called by either view or viewmodel
        /// </summary>
        public void Close()
        {
            if (_exited)
            {
                Closed?.Invoke(this, System.EventArgs.Empty);
                return;
            }
            _closingFromUi = true;
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
            Profile = shellProfile;

            if (!_requireShellProcessStart && !string.IsNullOrEmpty(termState))
            {
                OutputReceived?.Invoke(this, Encoding.UTF8.GetBytes(termState));
            }

            if (_requireShellProcessStart)
            {
                //var response = await _trayProcessCommunicationService
                //    .CreateTerminalAsync(Id, size, shellProfile, sessionType).ConfigureAwait(false);

                _session = new ConPtySession();
                _session.OutputReceived += Session_OutputReceived;
                _session.ConnectionClosed += Session_ConnectionClosed;

                _session.Start(new Models.Requests.CreateTerminalRequest { Id = Id, Profile = shellProfile, SessionType = SessionType.WinPty, Size = new TerminalSize { Rows = 20, Columns = 80 } });

                return new CreateTerminalResponse { Success = true };

                //if (response.Success)
                //{
                //    _fallbackTitle = response.Name;
                //    SetTitle(_fallbackTitle);
                //}
                //return response;
            }
            else
            {
                return await _trayProcessCommunicationService.PauseTerminalOutputAsync(Id, false);
            }
        }

        private void Session_ConnectionClosed(object sender, int e)
        {
        }

        private void Session_OutputReceived(object sender, TerminalOutput e)
        {
            Debug.WriteLine(Encoding.UTF8.GetString(e.Data));
            OutputReceived?.Invoke(this, e.Data);
        }

        public Task Write(byte[] data)
        {
            _session.Write(data);
            return Task.CompletedTask;
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