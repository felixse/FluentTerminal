using System;
using System.Windows.Input;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class MenuItemViewModel : MenuItemViewModelBase
    {
        #region Properties

        private MenuItemKeyBindingViewModel _keyBinding;

        public MenuItemKeyBindingViewModel KeyBinding
        {
            get => _keyBinding;
            set => Set(ref _keyBinding, value);
        }

        public ICommand Command { get; }

        #endregion Properties

        #region Constructors

        public MenuItemViewModel(string text, ICommand command, string description = null, object icon = null,
            MenuItemKeyBindingViewModel keyBinding = null) : base(text, description, icon)
        {
            _keyBinding = keyBinding;
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        #endregion Constructors

        #region Methods

        public override bool EquivalentTo(MenuItemViewModelBase other)
        {
            if (!base.EquivalentTo(other)) return false;

            if (!(other is MenuItemViewModel menuItem)) return false;

            return _keyBinding?.EquivalentTo(menuItem._keyBinding) ?? menuItem._keyBinding == null;
        }

        #endregion Methods
    }
}