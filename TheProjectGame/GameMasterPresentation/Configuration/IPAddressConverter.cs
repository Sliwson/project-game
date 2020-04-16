using System;
using System.Net;
using System.Windows.Data;

namespace GameMasterPresentation.Configuration
{
    public class IPAddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IPAddress ip)
            {
                return ip.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is string address)
            {
                if (IPAddress.TryParse(address, out IPAddress iPAddress))
                {
                    return iPAddress;
                }
            }
            return null;
        }
    }
}