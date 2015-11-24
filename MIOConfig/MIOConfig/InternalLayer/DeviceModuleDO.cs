using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    [Serializable]
    public class DeviceModuleDO
    {
        public DeviceModuleDO()
        {
            ModuleOperation = 1;
            PulseDurationTime = 10;
            ParallelChannelPresentForOutput1 = 0;
            ParallelChannelPresentForOutput2 = 0;
            ParallelChannelPresentForOutput3 = 0;
            ParallelChannelPresentForOutput4 = 0;
        }
        private Byte _moduleOperation;
        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+5 LoByte|count: 1| R/W
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
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+5 HiByte + 1007+7*DeviceUartPorts.Count+6 LoByte |count: 2| R/W
        /// </summary>
        /// <value>range: 0..65535</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 PulseDurationTime { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+6 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - out of operation, 1 - in operation</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ResevedByte1 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+7 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType1 { get; set; }

        private Byte _parallelChannelPresentForOutput1;
        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+7 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput1
        {
            get { return _parallelChannelPresentForOutput1; }
            set { _parallelChannelPresentForOutput1 = value > 1 ? (Byte)1 : value; }  
        } 

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+8|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput1 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+9 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType2 { get; set; }

        private Byte _parallelChannelPresentForOutput2;
        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+9 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput2
        {
            get { return _parallelChannelPresentForOutput1; }
            set { _parallelChannelPresentForOutput1 = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+10|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput2 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+11 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType3 { get; set; }

        private Byte _parallelChannelPresentForOutput3;
        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+11 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput3
        {
            get { return _parallelChannelPresentForOutput1; }
            set { _parallelChannelPresentForOutput1 = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+12|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput3 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+13 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType4 { get; set; }

        private Byte _parallelChannelPresentForOutput4;
        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+13 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput4
        {
            get { return _parallelChannelPresentForOutput1; }
            set { _parallelChannelPresentForOutput1 = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+14|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput4 { get; set; }
    }
}
