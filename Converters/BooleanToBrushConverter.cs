using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PrinterJobInterceptor.Converters;

public class BooleanToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string colors)
        {
            var colorArray = colors.Split('|');
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boolValue ? colorArray[0] : colorArray[1]));
            brush.Freeze();
            return brush;
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 