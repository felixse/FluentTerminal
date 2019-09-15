using System;

namespace FluentTerminal.Models.Messages
{
    public class ApplicationSettingsChangedMessage
    {
        public ApplicationSettings ApplicationSettings { get; }

        public ApplicationSettingsChangedMessage(ApplicationSettings applicationSettings)
        {
            ApplicationSettings = applicationSettings ?? throw new ArgumentNullException();
        }
    }
}