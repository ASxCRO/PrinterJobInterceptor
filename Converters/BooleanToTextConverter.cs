using System;
using System.Globalization;
using System.Windows.Data;

namespace PrinterJobInterceptor.Converters;

public class BooleanToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string text)
        {
            var options = text.Split('|');
            return boolValue ? options[0] : options[1];
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 