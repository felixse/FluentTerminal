using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class MenuItemViewModel : ViewModelBase
    {
        #region Properties

        private string _text;

        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }

        private string _description;

        public string Description
        {
            get => _description;
            set => Set(ref _description, value);
        }

        private MenuItemKeyBindingViewModel _keyBinding;

        public MenuItemKeyBindingViewModel KeyBinding
        {
            get => _keyBinding;
            set => Set(ref _keyBinding, value);
        }

        public RelayCommand Command { get; }

        #endregion Properties

        #region Constructors

        public MenuItemViewModel(RelayCommand command, string text, string description = null, MenuItemKeyBindingViewModel keyBinding = null)
        {
            _text = text;
            _description = description;
            _keyBinding = keyBinding;
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        #endregion Constructors

        #region Methods

        public bool EquivalentTo(MenuItemViewModel other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (other == null) return false;

            return string.Equals(_text, other._text, StringComparison.Ordinal) &&
                   string.Equals(_description, other._description, StringComparison.Ordinal) &&
                   (_keyBinding?.EquivalentTo(other._keyBinding) ?? other._keyBinding == null);
        }

        #endregion Methods
    }
}