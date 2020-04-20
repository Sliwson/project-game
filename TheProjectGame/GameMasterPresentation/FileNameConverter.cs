using System;
using System.Globalization;
using System.Windows.Data;

namespace GameMasterPresentation
{
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filename)
            {
                if(string.IsNullOrEmpty(filename))
                {
                    return "Internal configuration";
                }
                return filename;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}