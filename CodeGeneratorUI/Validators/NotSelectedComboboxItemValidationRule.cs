using System.Globalization;
using System.Windows.Controls;

namespace CodeGeneratorUI.Validators
{
    public class NotSelectedComboboxItemValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, Properties.Resources.you_must_select_an_option)
                : ValidationResult.ValidResult;
        }
    }
}
