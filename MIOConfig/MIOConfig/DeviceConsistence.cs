using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceConsistence
    {
        public bool ModuleModbusMaster;
        public bool ModuleRetranslator;
        public bool ModuleTS;
        public bool ModuleTU;
        public bool ModuleGPS;
        public bool ModuleMAX7219;
        public bool ModuleAdvantage_TY;
        public bool Reserve7;
        public bool Reserve8;
        public bool Reserve9;
        public bool Reserve10;
        public bool Reserve11;
        public bool Reserve12;
        public bool Reserve13;
        public bool Reserve14;
        public bool ModuleModbusSlave;
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 RegisterValue
        {
            get
            {               
                int numeral = 0;
                Byte i = 0;                     
                foreach (var field in GetType().GetFields())
                {
                    if ((bool) field.GetValue(this))                    
                        numeral |= 1 << i;                    
                    i++;
                }
                return (UInt16)numeral;
            }
            set
            {               
                Byte i = 0;
                foreach (var field in GetType().GetFields())
                {                   
                    field.SetValue(this, (value & (1 << i++)) != 0);                                                                
                }               
            } 
        }        
    }    
}
