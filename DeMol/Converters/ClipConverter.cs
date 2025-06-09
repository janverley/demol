using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DeMol.Converters
{
    public class ClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 2 && values[0] is double width && values[1] is double height)
            {
                var ellipseGeometry = new EllipseGeometry
                {
                    Center = new Point(width / 2, height / 2),
                    RadiusX = width / 2,
                    RadiusY = height / 2
                };
                return ellipseGeometry;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}