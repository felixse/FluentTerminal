using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentTerminal.App.ViewModels.Infrastructure
{
    /// <summary>
    /// based on: https://github.com/johnthiriet/AsyncVoid
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();
        bool CanExecute();
    }

    public interface IAsyncCommand<T> : ICommand
    {
        Task ExecuteAsync(T parameter);
        bool CanExecute(T parameter);
    }
}
