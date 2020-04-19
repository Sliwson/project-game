using System;
using System.Globalization;
using System.Windows.Data;

namespace GameMasterPresentation
{
    public class GMConfigToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Configuration.Configuration conf)
            {
                if (conf != null)
                {
                    return conf.Validate();
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}