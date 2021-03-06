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
                if (((BindingGroup)value).Items != null && ((BindingGroup)value).Items.Count>0)
                {
                    DeviceRoutingTableElement elementToValidate = ((BindingGroup)value).Items[0] as DeviceRoutingTableElement;
                    if (Validator != null && elementToValidate != null)
                    {
                        if (Validator.ValidateRoutingMapElement(elementToValidate) == false)
                        {
                            elementToValidate.RouteConfigured = false;
                            Validator.NotifyOnValidation();
                            return new ValidationResult(false, Validator.ToString());
                        }
                        elementToValidate.RouteConfigured = true;
                        Validator.NotifyOnValidation();
                    }  
                }                  
            }            
            return ValidationResult.ValidResult;            
        }
    }    
}