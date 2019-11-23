using FluentTerminal.Models;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class MenuItemKeyBindingViewModel : ViewModelBase
    {
        #region Properties

        private int _key;

        public int Key
        {
            get => _key;
            set => Set(ref _key, value);
        }

        private int _keyModifiers;

        // Corresponds to https://docs.microsoft.com/en-us/uwp/api/windows.system.virtualkeymodifiers
        public int KeyModifiers
        {
            get => _keyModifiers;
            private set => Set(ref _keyModifiers, value);
        }

        private bool _ctrl;

        public bool Ctrl
        {
            get => _ctrl;
            set
            {
                if (Set(ref _ctrl, value))
                {
                    SetKeyModifiers();
                }
            }
        }

        private bool _alt;

        public bool Alt
        {
            get => _alt;
            set
            {
                if (Set(ref _alt, value))
                {
                    SetKeyModifiers();
                }
            }
        }

        private bool _shift;

        public bool Shift
        {
            get => _shift;
            set
            {
                if (Set(ref _shift, value))
                {
                    SetKeyModifiers();
                }
            }
        }

        private bool _windows;

        public bool Windows
        {
            get => _windows;
            set
            {
                if (Set(ref _windows, value))
                {
                    SetKeyModifiers();
                }
            }
        }

        #endregion Properties

        #region Constructors

        public MenuItemKeyBindingViewModel(int key = 0, bool ctrl = false, bool alt = false, bool shift = false,
            bool windows = false)
        {
            _key = key;
            _ctrl = ctrl;
            _alt = alt;
            _shift = shift;
            _windows = windows;

            SetKeyModifiers();
        }

        public MenuItemKeyBindingViewModel(KeyBinding keyBinding) : this(keyBinding.Key, keyBinding.Ctrl,
            keyBinding.Alt, keyBinding.Shift, keyBinding.Meta)
        {
        }

        #endregion Constructors

        #region Methods

        private void SetKeyModifiers()
        {
            // Based on VirtualKeyModifiers enum values (https://docs.microsoft.com/en-us/uwp/api/windows.system.virtualkeymodifiers)
            var result = 0;

            if (_ctrl)
            {
                result |= 1;
            }

            if (_alt)
            {
                result |= 2;
            }

            if (_shift)
            {
                result |= 4;
            }

            if (_windows)
            {
                result |= 8;
            }

            KeyModifiers = result;
        }

        #endregion Methods
    }
}