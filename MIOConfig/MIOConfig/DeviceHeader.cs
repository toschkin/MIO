using System;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceHeader
    {
        private DeviceConfiguration _deviceConfiguration;        

        private UInt16 _deviceUartChannelsCount;

        public DeviceHeader()
        {
            _deviceConfiguration = null;
            _deviceUartChannelsCount = 1;
        }
        public DeviceHeader( ref DeviceConfiguration deviceConfiguration)
        {
            _deviceConfiguration = deviceConfiguration;
            _deviceUartChannelsCount = 1;
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
                if (_deviceConfiguration != null)
                {
                    if (value > _deviceConfiguration.DeviceUartPorts.Count)
                    {
                        int addCount = value - _deviceConfiguration.DeviceUartPorts.Count;

                        for (int i = 0; i < addCount; i++)
                            _deviceConfiguration.DeviceUartPorts.Add(new DeviceUARTPortConfiguration());
                    }

                    if (value < _deviceConfiguration.DeviceUartPorts.Count)
                        _deviceConfiguration.DeviceUartPorts.RemoveRange(value, _deviceConfiguration.DeviceUartPorts.Count - value);    
                }                
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