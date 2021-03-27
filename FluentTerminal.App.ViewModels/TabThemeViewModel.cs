using FluentTerminal.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentTerminal.App.ViewModels
{
    public class TabThemeViewModel : ObservableObject
    {
        private bool _isSelected;

        public TabThemeViewModel(TabTheme theme, TerminalViewModel terminal)
        {
            Theme = theme;
            Terminal = terminal;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value) && value)
                {
                    Terminal.TabTheme = this;
                }
            }
        }

        public TabTheme Theme { get; }
        public TerminalViewModel Terminal { get; }
    }
}
