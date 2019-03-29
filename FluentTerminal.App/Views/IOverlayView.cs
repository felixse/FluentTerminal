using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views
{
    interface IOverlayView
    {
        Task Initialize(OverlayViewModel viewModel);
    }
}
