using System;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    [Serializable]
    public class DeviceModuleDI
    {
        public DeviceModuleDI()
        {
            HysteresisTime = 10;
            ModuleOperation = 1;
        }
        private Byte _moduleOperation;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - out of operation, 1 - in operation</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ModuleOperation
        {
            get { return _moduleOperation; }
            set { _moduleOperation = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+1 HiByte + 1007+7*UartPorts.Count+2 LoByte |count: 2| R/W
        /// </summary>
        /// <value>range: 0..65535</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 HysteresisTime { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+2 HiByte|count: 1| R/W
        /// </summary>
        /// <value></value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ResevedByte1 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+3|count: 1| R/W
        /// </summary>
        /// <value></value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 ResevedWord1 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+4|count: 1| R/W
        /// </summary>
        /// <value></value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 ResevedWord2 { get; set; }
    }
}
