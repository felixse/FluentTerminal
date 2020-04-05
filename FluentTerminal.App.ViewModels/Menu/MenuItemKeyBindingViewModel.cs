using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using Windows.System;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class MenuItemKeyBindingViewModel : ViewModelBase
    {
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

        public bool IsExtendedVirtualKey => !Enum.IsDefined(typeof(VirtualKey), Key);

        public string GetOverrideText()
        {
            var segments = new List<string>();

            if (Ctrl) segments.Add("Ctrl");
            if (Alt) segments.Add("Alt");
            if (Windows) segments.Add("Windows");
            if (Shift) segments.Add("Shift");

            segments.Add(EnumHelper.GetEnumDescription((ExtendedVirtualKey)Key));

            return string.Join("+", segments);
        }

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

        public bool EquivalentTo(MenuItemKeyBindingViewModel other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (other == null) return false;

            return _key == other._key && _keyModifiers == other._keyModifiers;
        }

    }
}