using FluentTerminal.Models;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class MenuItemViewModelBase : ViewModelBase
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

        private object _icon;

        /// <summary>
        /// Defines menu item icon.
        /// </summary>
        /// <remarks>
        /// Acceptable values of this property are:
        /// <list type="bullet">
        /// <item><see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.iconelement">IconElement</see>
        /// (usually <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.symbolicon">SymbolIcon</see>
        /// or <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.fonticon">FontIcon</see>);</item>
        /// <item><see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.symbol">Symbol</see> enum value,
        /// which will be used for creating
        /// <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.symbolicon">SymbolIcon</see>;</item>
        /// <item><see cref="int"/>, in which case it'll be converted to
        /// <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.symbol">Symbol</see> enum value;</item>
        /// <item><see cref="string"/>, in which case a new
        /// <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.fonticon">FontIcon</see> instance will
        /// be created, whose
        /// <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.fonticon.glyph">Glyph</see> property
        /// will be set to the value of this property.
        /// <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.fonticon.fontfamily">FontFamily</see>
        /// property of the newly created instance will be set to <c>"Segoe MDL2 Assets"</c>.</item>
        /// </list>
        /// </remarks>
        public object Icon
        {
            get => _icon;
            set => Set(ref _icon, value);
        }

        #endregion Properties

        #region Constructor

        protected MenuItemViewModelBase(string text, string description, object icon)
        {
            _text = text;
            _description = description;
            _icon = icon;
        }

        #endregion Constructor

        #region Methods

        public virtual bool EquivalentTo(MenuItemViewModelBase other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (other == null) return false;

            return _text.NullableEqualTo(other._text) && _description.NullableEqualTo(other._description) &&
                   (_icon?.Equals(other._icon) ?? other._icon == null);
        }

        #endregion Methods
    }
}