using System;
using GalaSoft.MvvmLight.Command;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class MenuItemViewModel : MenuItemViewModelBase
    {
        #region Properties

        public MenuItemKeyBindingViewModel KeyBinding { get; }

        public RelayCommand Command { get; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor used for menu items which <strong>do have</strong> key binding associated.
        /// </summary>
        public MenuItemViewModel(MenuItemKeyBindingViewModel keyBinding, string text, string description = null,
            object icon = null) : base(text, description, icon)
        {
            KeyBinding = keyBinding ?? throw new ArgumentNullException(nameof(keyBinding));
            Command = keyBinding.Command;
        }

        /// <summary>
        /// Constructor used for menu items which <strong>do not have</strong> key binding associated.
        /// </summary>
        public MenuItemViewModel(RelayCommand command, string text, string description = null, object icon = null) :
            base(text, description, icon)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        #endregion Constructors
    }
}