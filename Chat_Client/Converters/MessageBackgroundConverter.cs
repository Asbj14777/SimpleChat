using Chat_Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Chat_Client.Converters
{
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Message msg)
            {
                if (msg.Type == MessageType.System)
                    return Brushes.Transparent;

                return msg.IsIncoming
                    ? new SolidColorBrush(Color.FromRgb(40, 167, 69))
                    : new SolidColorBrush(Color.FromRgb(0, 120, 215));
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
