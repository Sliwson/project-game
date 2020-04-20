using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameMasterPresentation
{
    public class GMConfigToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Configuration.Configuration conf)
            {
                if (conf != null)
                {
                    if (conf.Validate() == true)
                    {
                        return new SolidColorBrush(Colors.Green);
                    }
                }
            }
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}