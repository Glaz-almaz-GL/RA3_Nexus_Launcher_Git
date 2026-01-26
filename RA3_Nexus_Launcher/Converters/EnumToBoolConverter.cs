using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RA3_Nexus_Launcher.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return parameter is not null && (value?.Equals(parameter) ?? false);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool boolVal && boolVal && parameter is Enum enumVal ? enumVal : Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}