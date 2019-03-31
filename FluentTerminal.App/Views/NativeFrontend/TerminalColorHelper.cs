using FluentTerminal.App.Utilities;
using FluentTerminal.App.Views.NativeFrontend.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace FluentTerminal.App.Views.NativeFrontend
{
    internal class TerminalColorHelper
    {
        private readonly Dictionary<int, Brush> _brushDictionary = new Dictionary<int, Brush>();
        private readonly Brush _defaultForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));

        public Brush GetForegroundBrush(int id)
        {
            Brush brush = GetBrush(id);
            if (brush == null)
            {
                brush = _defaultForegroundBrush;
            }
            return brush;
        }

        public Brush GetBackgroundBrush(int id)
        {
            return GetBrush(id);
        }

        public Brush GetBrush(int id)
        {
            Brush result = null;
            if (id != 0)
            {
                if (!_brushDictionary.TryGetValue(id, out result))
                {
                    result = new SolidColorBrush(GetColour(id));
                    _brushDictionary.Add(id, result);
                }
            }
            return result;
        }

        public Color GetColour(int id)
        {
            switch (id)
            {
                case SpecialColorIds.Cursor:
                    return Color.FromArgb(255, 204, 204, 204);
                case SpecialColorIds.Selection:
                    return Color.FromArgb(255, 203, 203, 203);
                case SpecialColorIds.Historic:
                case SpecialColorIds.Futuristic:
                    return Color.FromArgb(255, 24, 24, 24);
                default:
                    return TangoColours[id % 16].FromString();
            }
        }

        // Colors 0-15
        private readonly static string[] TangoColours =
        {
            // dark:
            "#2e3436",
            "#cc0000",
            "#0dbc79",
            "#c4a000",
            "#3465a4",
            "#75507b",
            "#06989a",
            "#d3d7cf",

            // bright:
            "#555753",
            "#ef2929",
            "#23D18B",
            "#fce94f",
            "#729fcf",
            "#ad7fa8",
            "#34e2e2",
            "#eeeeec"
        };
    }
}
