using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    public class Device
    {       
        /// <summary>
        /// Holding regs|addr.: 1000|count: 1
        /// </summary>
        public DeviceConsistence DeviceParts;

        /// <summary>
        /// Holding regs|addr.: 1001|count: 1
        /// </summary>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 DeviceUartChannelsCount { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1002|count: 1| R/O
        /// </summary>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 DeviceUserRegistersCount { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1003|count: 1| R/O
        /// </summary>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 DeviceMaximumModbusMasterRequestsToSubDeviceCount { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1004|count: 1| R/O
        /// </summary>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]       
        public UInt16 DeviceHeaderCrc16 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1005|count: 2| R/W
        /// </summary>
        public DeviceConfigurationTime ConfigurationTime;
    }
}
