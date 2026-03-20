using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProfileIpSwitcher.Converters;

/// <summary>Standard Bool → Visibility (true = Visible).</summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
