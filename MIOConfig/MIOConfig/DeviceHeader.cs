using System;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceHeader
    {
        private DeviceConfiguration _deviceConfiguration;        

        private UInt16 _deviceUartChannelsCount;

        public DeviceHeader(DeviceConfiguration deviceConfiguration)
        {
            _deviceConfiguration = deviceConfiguration;
        }

        /// <summary>
        /// Holding regs|addr.: 1001|count: 1
        /// </summary>     
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]   
        public UInt16 DeviceUartChannelsCount 
        {
            get { return _deviceUartChannelsCount; }
            set
            {
                if (value < 1)
                    value = 1;
                if (value > _deviceConfiguration.DeviceUartPorts.Count)
                    _deviceConfiguration.DeviceUartPorts.AddRange(new DeviceUARTPortConfiguration[value - _deviceConfiguration.DeviceUartPorts.Count]);
                if (value < _deviceConfiguration.DeviceUartPorts.Count)
                    _deviceConfiguration.DeviceUartPorts.RemoveRange(value, _deviceConfiguration.DeviceUartPorts.Count - value);
                _deviceUartChannelsCount = value;                
            }
        }

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
    }
}