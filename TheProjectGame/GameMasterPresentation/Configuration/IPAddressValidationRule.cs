using System.Globalization;
using System.Net;
using System.Windows.Controls;

namespace GameMasterPresentation.Configuration
{
    public class IPAddressValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string s)
            {
                if (IPAddress.TryParse(s, out IPAddress address) == true)
                {
                    return ValidationResult.ValidResult;
                }
            }
            return new ValidationResult(false, "Value should be valid IP address!");
        }
    }
}