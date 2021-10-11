using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace FluentTerminal.App.Utilities
{
    public static class ContrastHelper
    {
        public static ElementTheme GetIdealThemeForBackgroundColor(string color)
        {
            return GetIdealThemeForBackgroundColor(Colors.Red); // color.ToColor()
        }

        public static ElementTheme GetIdealThemeForBackgroundColor(Color color)
        {
            if (((color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114)) > 186)
            {
                return ElementTheme.Light;
            }
            else
            {
                return ElementTheme.Dark;
            }
        }

        public static void SetTitleBarButtonsForTheme(ElementTheme theme)
        {
            //var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            //titleBar.ButtonBackgroundColor = Colors.Transparent;
            //titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            //titleBar.ButtonForegroundColor = GetColor("SystemBaseHighColor", theme);
            //titleBar.ButtonInactiveForegroundColor = GetColor("SystemBaseHighColor", theme);
            //titleBar.ButtonHoverForegroundColor = GetColor("SystemBaseHighColor", theme);
            //titleBar.ButtonPressedForegroundColor = GetColor("SystemBaseHighColor", theme);

            //titleBar.ButtonHoverBackgroundColor = GetColor("SystemListLowColor", theme);
            //titleBar.ButtonPressedBackgroundColor = GetColor("SystemListMediumColor", theme);
        }

        public static Color? GetColor(string name, ElementTheme theme)
        {
            if (theme == ElementTheme.Light)
            {
                switch (name)
                {
                    case "SystemBaseHighColor":
                        return new Color { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 };

                    case "SystemListLowColor":
                        return new Color { A = 0x19, R = 0x00, G = 0x00, B = 0x00 };

                    case "SystemListMediumColor":
                        return new Color { A = 0x33, R = 0x00, G = 0x00, B = 0x00 };

                    default:
                        return null;
                }
            }
            else
            {
                switch (name)
                {
                    case "SystemBaseHighColor":
                        return new Color { A = 0xFF, R = 0xFF, G = 0xFF, B = 0xFF };

                    case "SystemListLowColor":
                        return new Color { A = 0x19, R = 0xFF, G = 0xFF, B = 0xFF };

                    case "SystemListMediumColor":
                        return new Color { A = 0x33, R = 0xFF, G = 0xFF, B = 0xFF };

                    default:
                        return null;
                }
            }
        }
    }
}