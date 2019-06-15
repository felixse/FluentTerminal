using System;

namespace FluentTerminal.App.ViewModels.Infrastructure
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
