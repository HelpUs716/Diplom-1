using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Учет.Converters
{
    public class DateToExpirationColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime plannedDate)
            {
                var daysLeft = (plannedDate.Date - DateTime.Today).TotalDays;
                if (daysLeft < 0)
                    return new SolidColorBrush(Color.FromRgb(255, 99, 71)); 
                if (daysLeft <= 7)
                    return new SolidColorBrush(Color.FromRgb(255, 215, 0));
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}