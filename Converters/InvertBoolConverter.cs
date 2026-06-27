using System.Globalization;

namespace LineaClient.Converters
{
    // Invierte un bool: usado para deshabilitar el boton mientras carga
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}
