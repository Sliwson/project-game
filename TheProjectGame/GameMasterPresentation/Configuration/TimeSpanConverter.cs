using System;
using System.Windows.Data;

namespace GameMasterPresentation.Configuration
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan time)
            {
                return time.TotalMilliseconds;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string s)
            {
                if (int.TryParse(s, out int time))
                {
                    return TimeSpan.FromMilliseconds(time);
                }
            }

            return null;
        }
    }
}