using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Data;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Converters
{
    public class LineEndingStyleConverter : IValueConverter
    {
        #region Static

        public static ObservableCollection<LineEndingStyle> LineEndingStyles { get; } =
            new ObservableCollection<LineEndingStyle>((LineEndingStyle[]) Enum.GetValues(typeof(LineEndingStyle)));

        private static readonly Lazy<Dictionary<LineEndingStyle, string>> LineEndingNames =
            new Lazy<Dictionary<LineEndingStyle, string>>(() =>
                LineEndingStyles.ToDictionary(les => les, les => GetEnumDescription(les)));

        private static string GetEnumDescription(Enum enumObj)
        {
            if (enumObj == null)
                return null;

            return enumObj.GetType().GetField(enumObj.ToString()).GetCustomAttribute<DescriptionAttribute>()
                       ?.Description ?? enumObj.ToString();
        }

        #endregion Static

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is LineEndingStyle lineEnding))
                throw new ArgumentException($"Input value has to be of type {nameof(LineEndingStyle)}", nameof(value));

            if (targetType != typeof(string))
                throw new ArgumentException("Target type has to be string.", nameof(targetType));

            return LineEndingNames.Value[lineEnding];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}