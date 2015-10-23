using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Core
{
    public class ModbusDataPoint<T>         
       where T : struct 
    {
        public ModbusDataPoint()
        {            
            Value = default(T);
        }        
        public T Value { get; set; }       
    }
}
