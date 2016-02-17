using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;
using System.Reflection;

namespace MIOConfig
{
    public class DeviceDIModuleStatus
    {
        public bool ErrorCurrent { get; set; }
        public bool ErrorVoltage { get; set; }
        public bool ErrorCheck { get; set; }
        public bool Reserve3{ get; set; }
        public bool Reserve4{ get; set; }
        public bool Reserve5{ get; set; }
        public bool Reserve6{ get; set; }
        public bool Reserve7{ get; set; }
        public bool Reserve8{ get; set; }
        public bool Reserve9{ get; set; }
        public bool Reserve10{ get; set; }
        public bool Reserve11{ get; set; }
        public bool Reserve12{ get; set; }
        public bool Reserve13{ get; set; }
        public bool Reserve14{ get; set; }
        public bool Reserve15{ get; set; }
        /// <summary>
        /// Holding regs|addr.: 505|count: 1
        /// </summary>     
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 StatusRegister
        {
            get
            {
                int numeral = 0;
                Byte i = 0;
                foreach (var field in OrderedGetter.GetObjectPropertiesInDeclarationOrder(this))
                {
                    if (field.GetCustomAttributes(typeof(ModbusPropertyAttribute), false).Length > 0)//except itself
                        continue;

                    if ((bool)field.GetValue(this,null))
                        numeral |= 1 << i;
                    i++;
                }
                return (UInt16)numeral;
            }
            set
            {
                Byte i = 0;
                foreach (var field in OrderedGetter.GetObjectPropertiesInDeclarationOrder(this))
                {
                    if(field.GetCustomAttributes(typeof (ModbusPropertyAttribute), false).Length > 0)//except itself
                        continue;
                    field.SetValue(this, (value & (1 << i++)) != 0, null);
                }
            }
        }
    }
}
