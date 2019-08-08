using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FluentTerminal.SystemTray
{
    public static class FileFinder
    {
        public static string GetCommandPath(this string command)
        {
            var procLock = new object();

            string path = null;
            string error = null;

            using(var process = new Process())
            using (var mre = new ManualResetEvent(false))
            {
                process.StartInfo.FileName = "where";
                process.StartInfo.Arguments = command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += (sender, e) =>
                {
                    lock (procLock)
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            path = e.Data.Trim();

                            // ReSharper disable once AccessToDisposedClosure
                            mre.Set();
                        }
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    lock (procLock)
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            error = e.Data.Trim();

                            // ReSharper disable once AccessToDisposedClosure
                            mre.Set();
                        }
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                mre.WaitOne(50);

                lock (procLock)
                {
                    if (process.ExitCode == 0)
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            throw new Exception("Should not happen! 'where' exit code is 0, yet the path is empty.");
                        }

                        if (!File.Exists(path))
                        {
                            throw new Exception(
                                "Should not happen! 'where' returned a path that doesn't exist: " + path);
                        }

                        return path;
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception($"'{command}' command not found. Error message: " + error);
                    }

                    throw new Exception($"'{command}' command not found. Exit code: {process.ExitCode:##########}");
                }
            }
        }
    }
}