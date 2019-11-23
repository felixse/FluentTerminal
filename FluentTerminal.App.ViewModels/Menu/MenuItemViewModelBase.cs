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
        /// Has to be of type <see cref="!:https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.iconelement">IconElement</see>.
        /// </summary>
        public object Icon
        {
            get => _icon;
            set => Set(ref _icon, value);
        }

        #endregion Properties

        #region Constructors

        protected MenuItemViewModelBase(string text, string description = null, object icon = null)
        {
            _text = text;
            _description = description;
            _icon = icon;
        }

        #endregion Constructors
    }
}