using FluentTerminal.App.Exceptions;
using FluentTerminal.Models;
using Microsoft.Toolkit.Uwp.Helpers;
using PListNet;
using PListNet.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace FluentTerminal.App.Services.Implementation
{
    public class ITermThemeParser : IThemeParser
    {
        public IEnumerable<string> SupportedFileTypes { get; } = new string[] { ".itermcolors" };

        private static class ITermThemeKeys
        {
            public const string Ansi0Color = "Ansi 0 Color";
            public const string Ansi1Color = "Ansi 1 Color";
            public const string Ansi2Color = "Ansi 2 Color";
            public const string Ansi3Color = "Ansi 3 Color";
            public const string Ansi4Color = "Ansi 4 Color";
            public const string Ansi5Color = "Ansi 5 Color";
            public const string Ansi6Color = "Ansi 6 Color";
            public const string Ansi7Color = "Ansi 7 Color";
            public const string Ansi8Color = "Ansi 8 Color";
            public const string Ansi9Color = "Ansi 9 Color";
            public const string Ansi10Color = "Ansi 10 Color";
            public const string Ansi11Color = "Ansi 11 Color";
            public const string Ansi12Color = "Ansi 12 Color";
            public const string Ansi13Color = "Ansi 13 Color";
            public const string Ansi14Color = "Ansi 14 Color";
            public const string Ansi15Color = "Ansi 15 Color";

            public const string BackgroundColor = "Background Color";
            public const string BoldColor = "Bold Color";
            public const string CursorColor = "Cursor Color";
            public const string CursorTextColor = "Cursor Text Color";
            public const string ForegroundColor = "Foreground Color";
            public const string SelectedTextColor = "Selected Text Color";
            public const string SelectionColor = "Selection Color";
        }

        private static class ITermThemeColorKeys
        {
            public const string BlueComponent = "Blue Component";
            public const string GreenComponent = "Green Component";
            public const string RedComponent = "Red Component";
        }

        public async Task<TerminalTheme> Parse(StorageFile themeFile)
        {
            var stream = await themeFile.OpenStreamForReadAsync().ConfigureAwait(false);
            var node = PList.Load(stream) as DictionaryNode ?? throw new ParseThemeException("Root node was not a dictionary.");

            return new TerminalTheme
            {
                Name = Path.GetFileNameWithoutExtension(themeFile.Name),
                Colors = GetColors(node),
                Id = Guid.NewGuid(),
                PreInstalled = false
            };
        }

        private TerminalColors GetColors(DictionaryNode themeDictionary)
        {
            return new TerminalColors
            {
                Background = GetColorString(themeDictionary[ITermThemeKeys.BackgroundColor]),
                Foreground = GetColorString(themeDictionary[ITermThemeKeys.ForegroundColor]),
                Cursor = GetColorString(themeDictionary[ITermThemeKeys.CursorColor]),
                CursorAccent = GetColorString(themeDictionary[ITermThemeKeys.CursorTextColor]),
                Selection = GetColorString(themeDictionary[ITermThemeKeys.SelectionColor]),
                Black = GetColorString(themeDictionary[ITermThemeKeys.Ansi0Color]),
                Red = GetColorString(themeDictionary[ITermThemeKeys.Ansi1Color]),
                Green = GetColorString(themeDictionary[ITermThemeKeys.Ansi2Color]),
                Yellow = GetColorString(themeDictionary[ITermThemeKeys.Ansi3Color]),
                Blue = GetColorString(themeDictionary[ITermThemeKeys.Ansi4Color]),
                Magenta = GetColorString(themeDictionary[ITermThemeKeys.Ansi5Color]),
                Cyan = GetColorString(themeDictionary[ITermThemeKeys.Ansi6Color]),
                White = GetColorString(themeDictionary[ITermThemeKeys.Ansi7Color]),
                BrightBlack = GetColorString(themeDictionary[ITermThemeKeys.Ansi8Color]),
                BrightRed = GetColorString(themeDictionary[ITermThemeKeys.Ansi9Color]),
                BrightGreen = GetColorString(themeDictionary[ITermThemeKeys.Ansi10Color]),
                BrightYellow = GetColorString(themeDictionary[ITermThemeKeys.Ansi11Color]),
                BrightBlue = GetColorString(themeDictionary[ITermThemeKeys.Ansi12Color]),
                BrightMagenta = GetColorString(themeDictionary[ITermThemeKeys.Ansi13Color]),
                BrightCyan = GetColorString(themeDictionary[ITermThemeKeys.Ansi14Color]),
                BrightWhite = GetColorString(themeDictionary[ITermThemeKeys.Ansi15Color]),
            };
        }

        private string GetColorString(PNode colorNode)
        {
            var dictionaryNode = colorNode as DictionaryNode ?? throw new ParseThemeException("Color node was not a dictionary");

            var red = dictionaryNode[ITermThemeColorKeys.RedComponent] as RealNode ?? throw new ParseThemeException("Red node value was not a real number");
            var green = dictionaryNode[ITermThemeColorKeys.GreenComponent] as RealNode ?? throw new ParseThemeException("Green node value was not a real number");
            var blue = dictionaryNode[ITermThemeColorKeys.BlueComponent] as RealNode ?? throw new ParseThemeException("Blue node value was not a real number");

            var color = Color.FromArgb(Byte.MaxValue, GetByteValue(red), GetByteValue(green), GetByteValue(blue));

            return color.ToHex();
        }

        private byte GetByteValue(RealNode node)
        {
            var doubleValue = node.Value * Byte.MaxValue;

            return Convert.ToByte(doubleValue);
        }
    }
}
