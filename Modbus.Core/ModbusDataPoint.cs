using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Modbus.Core
{
    public enum ModbusRegisterAccessType
    {
        AccessRead = 0,
        AccessReadWrite = 1
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ModbusPropertyAttribute : Attribute
    {
        public ModbusPropertyAttribute()
        {
            Access = ModbusRegisterAccessType.AccessReadWrite;
        }
        public ModbusRegisterAccessType Access { get; set; }        
    }

    public class ModbusDataPoint<T>         
       where T : struct 
    {
        public ModbusDataPoint()
        {            
            Value = default(T);
        }
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public T Value { get; set; }       

        public static implicit operator T(ModbusDataPoint<T> value)
        {
            return value.Value;
        }
    }
}
