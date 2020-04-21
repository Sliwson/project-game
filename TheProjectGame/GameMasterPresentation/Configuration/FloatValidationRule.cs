using System.Globalization;
using System.Windows.Controls;

namespace GameMasterPresentation.Configuration
{
    public class FloatValidationRule : ValidationRule
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string s)
            {
                if (float.TryParse(s, NumberStyles.Float, Constants.Culture, out float number) == true)
                {
                    if (number >= Min && number <= Max)
                        return ValidationResult.ValidResult;
                    else
                        return new ValidationResult(false, $"Value must be between {Min} and {Max}!");
                }
            }
            return new ValidationResult(false, "Value should be decimal!");
        }
    }
}