using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LibraryManagementFE.Converters
{
    /// <summary>Converts #RRGGBB / #AARRGGBB strings to <see cref="SolidColorBrush"/> for XAML bindings.</summary>
    [ValueConversion(typeof(string), typeof(SolidColorBrush))]
    public class HexBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch (FormatException) { /* ignore invalid hex */ }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Converts a relative height ratio (0–1) to a pixel height.
    /// ConverterParameter = max pixel height (default 220).
    /// Usage: Height="{Binding RelativeHeight, Converter={StaticResource PercentHeightConverter}, ConverterParameter=220}"
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class PercentHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double ratio)
            {
                double maxHeight = parameter is string s && double.TryParse(s, out double p) ? p : 220d;
                return Math.Max(4d, ratio * maxHeight);   // minimum 4px so bar is always visible
            }
            return 4d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Converts a TransactionStatus enum to a localised string.
    /// (Fallback – the model's StatusText property is used directly in bindings.)
    /// </summary>
    [ValueConversion(typeof(Models.TransactionStatus), typeof(string))]
    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.TransactionStatus status)
                return status switch
                {
                    Models.TransactionStatus.DaTra    => "Đã trả",
                    Models.TransactionStatus.DangMuon => "Đang mượn",
                    Models.TransactionStatus.QuaHan   => "Quá hạn",
                    _                                 => string.Empty
                };
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
