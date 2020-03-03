using FluentTerminal.Models;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels
{
    public class TabThemeViewModel : ViewModelBase
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
                if (Set(ref _isSelected, value) && value)
                {
                    Terminal.TabTheme = this;
                }
            }
        }

        public TabTheme Theme { get; }
        public TerminalViewModel Terminal { get; }
    }
}
