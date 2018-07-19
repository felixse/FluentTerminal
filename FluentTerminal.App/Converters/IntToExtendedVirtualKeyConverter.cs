using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models.Enums;
using System;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class IntToExtendedVirtualKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return EnumHelper.GetEnumDescription((ExtendedVirtualKey)intValue);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}