using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Chat_Client.Converters
{
    public class IncomingToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool incoming = (bool)value;

            return incoming 
                ? new SolidColorBrush(Color.FromRgb(40, 167, 69))
                : new SolidColorBrush(Color.FromRgb(0, 120, 215));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
