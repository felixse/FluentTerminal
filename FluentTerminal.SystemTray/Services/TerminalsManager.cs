﻿using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using FluentTerminal.SystemTray.Services.ConPty;
using FluentTerminal.SystemTray.Services.WinPty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using FluentTerminal.Models.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace FluentTerminal.SystemTray.Services
{
    public struct TerminalSessionInfo
    {
        public DateTime StartTime { get; set; }
        public string ProfileName { get; set; }
        public ITerminalSession Session { get; set; }
    }

    public class TerminalsManager
    {
        private readonly Dictionary<byte, TerminalSessionInfo> _terminals = new Dictionary<byte, TerminalSessionInfo>();
        private readonly ICommunicationServerService _dataServer;

        public event EventHandler<TerminalExitStatus> TerminalExited;

        private static readonly Regex EscapeSequencePattern = new Regex(@"((\x9B|\x1B\[)[0-?]*[ -\/]*[@-~])|((\x9D|\x1B\]).*\x07)", RegexOptions.Compiled);

        private Dictionary<byte, string> _cachedLogPath = new Dictionary<byte, string>();

        private ApplicationSettings _applicationSettings;

        public TerminalsManager(ISettingsService settingsService, ICommunicationServerService dataServer)
        {
            _applicationSettings = settingsService.GetApplicationSettings();
            _dataServer = dataServer;
            Messenger.Default.Register<ApplicationSettingsChangedMessage>(this, OnApplicationSettingsChanged);
        }

        private void OnApplicationSettingsChanged(ApplicationSettingsChangedMessage message)
        {
            _applicationSettings = message.ApplicationSettings;
        }

        public void DisplayTerminalOutput(byte terminalId, byte[] output)
        {
            _dataServer.SendTerminalDataEvent(terminalId, output);

            if (_applicationSettings.EnableLogging && Directory.Exists(_applicationSettings.LogDirectoryPath))
            {
                var logOutput = output;
                if (_applicationSettings.PrintableOutputOnly)
                {
                    string strOutput = System.Text.Encoding.UTF8.GetString(logOutput);
                    strOutput = EscapeSequencePattern.Replace(strOutput, "");
                    logOutput = Encoding.UTF8.GetBytes(strOutput);
                }

                try
                {
                    using (var logFileStream = System.IO.File.Open(GetLogFilePath(terminalId), System.IO.FileMode.Append))
                    {
                        logFileStream.Write(logOutput, 0, logOutput.Length);
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Debug("DisplayTerminalOutput failed. Exception: {0}", e);
                }
            }
        }

        private string GetLogFilePath(byte terminalId)
        {
            if (_terminals.ContainsKey(terminalId) == false)
                return String.Empty;

            if (_cachedLogPath.ContainsKey(terminalId) == false)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(_applicationSettings.LogDirectoryPath);
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(_terminals[terminalId].StartTime.ToString("yyyyMMddhhmmssfff"));
                sb.Append("_");
                sb.Append(_terminals[terminalId].ProfileName);
                sb.Append(".log");

                _cachedLogPath.Add(terminalId, sb.ToString());
            }

            return _cachedLogPath[terminalId];
        }

        public CreateTerminalResponse CreateTerminal(CreateTerminalRequest request)
        {
            if (_terminals.ContainsKey(request.Id))
            {
                // App terminated without cleaning up, removing orphaned sessions
                foreach (var item in _terminals.Values)
                {
                    item.Session.Dispose();
                }
                _terminals.Clear();
            }

            request.Profile.Location = Utilities.ResolveLocation(request.Profile.Location);

            ITerminalSession terminal = null;
            try
            {
                if (request.SessionType == SessionType.WinPty)
                {
                    terminal = new WinPtySession();
                }
                else
                {
                    terminal = new ConPtySession();
                }
                terminal.Start(request, this);
            }
            catch (Exception e)
            {
                return new CreateTerminalResponse { Error = e.ToString() };
            }

            terminal.ConnectionClosed += OnTerminalConnectionClosed;
            _terminals.Add(terminal.Id, new TerminalSessionInfo
            {
                ProfileName = String.IsNullOrEmpty(request.Profile.Name) ? terminal.ShellExecutableName : request.Profile.Name,
                StartTime = DateTime.Now,
                Session = terminal
            });
            return new CreateTerminalResponse
            {
                Success = true,
                ShellExecutableName = terminal.ShellExecutableName
            };
        }

        public void Write(byte id, byte[] data)
        {
            if (_terminals.TryGetValue(id, out TerminalSessionInfo sessionInfo))
            {
                try
                {
                    sessionInfo.Session.Write(data);
                }
                catch (IOException e)
                {
                    Logger.Instance.Error($"TerminalsManager.Write: sending user input to terminal with id '{id}' failed with exception: {e}");
                }
            }
        }

        public void ResizeTerminal(byte id, TerminalSize size)
        {
            if (_terminals.TryGetValue(id, out TerminalSessionInfo sessionInfo))
            {
                try
                {
                    sessionInfo.Session.Resize(size);
                }
                catch (Exception e)
                {
                    Logger.Instance.Error($"ResizeTerminal: resizing of terminal with id '{id}' failed with exception: {e}");
                }
            }
            else
            {
                Debug.WriteLine($"ResizeTerminal: terminal with id '{id}' was not found");
            }
        }

        public void CloseTerminal(byte id)
        {
            if (_terminals.TryGetValue(id, out TerminalSessionInfo sessionInfo))
            {
                _terminals.Remove(sessionInfo.Session.Id);
                sessionInfo.Session.Close();
            }
        }

        public PauseTerminalOutputResponse PauseTermimal(byte id, bool pause)
        {
            var response = new PauseTerminalOutputResponse()
            {
                Success = true
            };
            if (_terminals.TryGetValue(id, out TerminalSessionInfo sessionInfo))
            {
                sessionInfo.Session.Pause(pause);
            }
            return response;
        }

        public string GetDefaultEnvironmentVariableString(Dictionary<string, string> additionalVariables)
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            environmentVariables["TERM_PROGRAM"] = "FluentTerminal";
            environmentVariables["TERM_PROGRAM_VERSION"] = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

            if (additionalVariables != null)
            {
                foreach (var kvp in additionalVariables)
                {
                    environmentVariables[kvp.Key] = kvp.Value;
                }
            }

            var builder = new StringBuilder();

            foreach (DictionaryEntry item in environmentVariables)
            {
                builder.Append(item.Key).Append("=").Append(item.Value).Append("\0");
            }
            builder.Append('\0');

            return builder.ToString();
        }

        private void OnTerminalConnectionClosed(object sender, int exitcode)
        {
            if (sender is ITerminalSession terminal)
            {
                _terminals.Remove(terminal.Id);
                TerminalExited?.Invoke(this, new TerminalExitStatus(terminal.Id, exitcode));
                terminal.Dispose();
            }
        }
    }
}
