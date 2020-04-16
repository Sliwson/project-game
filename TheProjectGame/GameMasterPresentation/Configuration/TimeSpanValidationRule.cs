using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace GameMasterPresentation.Configuration
{
    public class TimeSpanValidationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string s)
            {
                if (int.TryParse(s, out int number) == true)
                {
                    if (number >= Min && number <= Max)
                        return ValidationResult.ValidResult;
                    else
                        return new ValidationResult(false, $"Value must be between {Min} and {Max}!");
                }
            }
            return new ValidationResult(false, "Value should be integer!");
        }
    }
}
