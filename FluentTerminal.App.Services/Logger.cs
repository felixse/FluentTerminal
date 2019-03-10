using Serilog;
using System;

namespace FluentTerminal.App.Services
{
    public sealed class Logger
    {
        public static Logger Instance { get; } = new Logger();

        private Logger()
        {

        }

        ~Logger()
        {
            Log.CloseAndFlush();
        }

        public void Initialize(string filePath)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(a => a.File(filePath))
            .MinimumLevel.Debug()
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