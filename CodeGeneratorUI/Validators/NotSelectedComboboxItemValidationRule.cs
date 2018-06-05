using System.Globalization;
using System.Windows.Controls;

namespace CodeGeneratorUI.Validators
{
    public class NotSelectedComboboxItemValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "You must select an option.")
                : ValidationResult.ValidResult;
        }
    }
}
