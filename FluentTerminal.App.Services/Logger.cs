using Serilog;
using Serilog.Events;
using System;

namespace FluentTerminal.App.Services
{
    public sealed class Logger
    {
        public enum LogLevel
        {
            Verbose = 0,
            Debug = 1,
            Information = 2,
            Warning = 3,
            Error = 4,
            Fatal = 5
        }

        public class Configuration
        {
            public LogLevel LogLevel { get; set; } = LogLevel.Error;
        }

        public static Logger Instance { get; } = new Logger();

        private Logger()
        {

        }

        public void Initialize(string filePath, Configuration configuration)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(a => a.File(filePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7))
            .MinimumLevel.Is((LogEventLevel)configuration.LogLevel)
            .CreateLogger();

            Log.Information("Initialized");
        }

        public void Debug(string text, params object[] propertyValues)
        {
            Log.Debug(text, propertyValues, propertyValues);
        }

        public void Information(string text, params object[] propertyValues)
        {
            Log.Information(text, propertyValues);
        }

        public void Warning(string text, params object[] propertyValues)
        {
            Log.Warning(text, propertyValues);
        }

        public void Error(string text, params object[] propertyValues)
        {
            Log.Error(text, propertyValues);
        }

        public void Error(Exception exception, string text, params object[] propertyValues)
        {
            Log.Error(exception, text, propertyValues);
        }
    }
}