using System.Windows.Controls;
using System.Windows.Data;
using MIOConfig;
using MIOConfig.PresentationLayer;

namespace MIOConfigurator.Data
{
    public class RoutingElementValidationRule : ValidationRule
    {
        public DeviceValidator Validator { get; set; }

        public override ValidationResult Validate(object value,System.Globalization.CultureInfo cultureInfo)
        {
            if (value is BindingGroup)
            {
                DeviceRoutingTableElement elementToValidate = ((BindingGroup) value).Items[0] as DeviceRoutingTableElement;
                if (Validator != null)
                {
                    if (Validator.ValidateRoutingMapElement(elementToValidate) == false)
                    {
                        return new ValidationResult(false, Validator.ToString());
                    }
                }    
            }            
            return ValidationResult.ValidResult;            
        }
    }    
}