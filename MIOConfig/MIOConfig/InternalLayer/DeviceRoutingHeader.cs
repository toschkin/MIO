using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
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
                 {
                     return (UInt16)_deviceConfiguration.RoutingTable.Aggregate(0, (current, query) => query.RouteConfigured ? current + 1 : current);
                     // was  return (UInt16)_deviceConfiguration.RoutingTable.Count;
                 }                                      
                 return 0;
             }
             set
             {
                 if (_deviceConfiguration != null)
                 {
                     int newValue = value;
                     if (newValue > _deviceConfiguration.RoutingTable.Count)
                         newValue = _deviceConfiguration.RoutingTable.Count;
                     for (int route = 0; route < newValue; route++)
                     {
                         _deviceConfiguration.RoutingTable[route].RouteConfigured = true;
                     }
                     for (int route = newValue; route < _deviceConfiguration.RoutingTable.Count; route++)
                     {
                         _deviceConfiguration.RoutingTable[route].RouteConfigured = false;
                     }
                     /*if (value > _deviceConfiguration.RoutingTable.Count)
                     {
                         int addCount = value - _deviceConfiguration.RoutingTable.Count;

                         for (int i = 0; i < addCount; i++)
                             _deviceConfiguration.RoutingTable.Add(new DeviceRoutingTableElement());                                                  
                     }
                    
                    _deviceConfiguration.RoutingTable.ToList().RemoveRange(value, _deviceConfiguration.RoutingTable.Count - value);    */
                 }                 
                 _routingTableSize = value;
             }
        }
        
    }
}
