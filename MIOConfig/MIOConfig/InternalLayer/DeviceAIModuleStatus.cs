using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceAIModuleStatus
    {        
        /// <summary>
        /// Holding regs|addr.: 528|count: 1
        /// </summary>     
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 StatusRegister { get; set; }
    }
}
