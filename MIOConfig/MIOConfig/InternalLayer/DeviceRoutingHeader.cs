using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    [Serializable]
    public class DeviceRoutingHeader
    {
        private DeviceConfiguration _deviceConfiguration;
        public DeviceRoutingHeader()
        {
            RoutingEnabled = 1;
            _deviceConfiguration = null;            
        }

        public DeviceRoutingHeader(ref DeviceConfiguration deviceConfiguration)
        {
            _deviceConfiguration = deviceConfiguration;
            RoutingEnabled = 1;
        }
        
        private UInt16 _routingEnabled;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count + 5*ModuleDIPresent + 10*ModuleDOPresent |count: 1| R/W
        /// </summary>
        /// <value>0 - routing disabled, 1 - routing enabled</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 RoutingEnabled
        {
            get { return _routingEnabled; }
            set
            {
                _routingEnabled = value > 1 ? (UInt16)1 : value;
            }
        }

        private UInt16 _routingTableSize;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count + 5*ModuleDIPresent + 10*ModuleDOPresent +1 |count: 1| R/W
        /// </summary>
        /// <value>0 - out of operation, 1 - in operation</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 RoutingTableSize
        {
             get
             {
                 if (_deviceConfiguration != null)                 
                     return (UInt16)_deviceConfiguration.RoutingTable.Count;
                 
                 return 0;
             }
             set
             {
                 if (_deviceConfiguration != null)
                 {
                     if (value > _deviceConfiguration.RoutingTable.Count)
                     {
                         int addCount = value - _deviceConfiguration.RoutingTable.Count;

                         for (int i = 0; i < addCount; i++)
                             _deviceConfiguration.RoutingTable.Add(new DeviceRoutingTableElement());
                     }

                     if (value < _deviceConfiguration.RoutingTable.Count)
                         _deviceConfiguration.RoutingTable.RemoveRange(value, _deviceConfiguration.RoutingTable.Count - value);    
                 }                 
                 _routingTableSize = value;
             }
        }
        
    }
}
