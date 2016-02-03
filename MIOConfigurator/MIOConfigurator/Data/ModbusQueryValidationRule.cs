using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using MIOConfig;
using MIOConfig.PresentationLayer;

namespace MIOConfigurator.Data
{
    class ModbusQueryValidationRule : ValidationRule
    {
        public DeviceValidator Validator { get; set; }
        public DeviceModbusMasterQuery SelectedQuery { get; set; }
        public string FieldNameToValidate { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            DeviceModbusMasterQuery elementToValidate = SelectedQuery;

            if (Validator != null && elementToValidate != null && value is string)
            {
                if (FieldNameToValidate == "RouteStartAddress")                
                    elementToValidate.RouteStartAddress = Convert.ToUInt16((string)value);
                if (FieldNameToValidate == "QueryStatusAddress")
                    elementToValidate.QueryStatusAddress = Convert.ToUInt16((string)value);
                if (FieldNameToValidate == "RegistersCount")
                    elementToValidate.RegistersCount = Convert.ToUInt16((string)value);  
                if (Validator.ValidateModbusMasterQuery(elementToValidate) == false)
                {
                    Validator.NotifyOnValidation();
                    return new ValidationResult(false, Validator.ToString());
                }
                Validator.NotifyOnValidation();
            }          
            return ValidationResult.ValidResult;
        }
    }              
}
