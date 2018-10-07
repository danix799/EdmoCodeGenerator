using System.Globalization;
using System.Windows.Controls;

namespace CodeGeneratorUI.Validators
{
   public class NotEmptyValidationRule: ValidationRule
    {
       public override ValidationResult Validate(object value, CultureInfo cultureInfo){
			return string.IsNullOrWhiteSpace((value ?? "").ToString())
				? new ValidationResult(false, Properties.Resources.field_is_required)
				: ValidationResult.ValidResult;
		}
    }
}
