using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceRoutingTableElement
    {
        public DeviceRoutingTableElement()
        {
            RouteConfigured = false;
        }        
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count + 5*ModuleDIPresent + 10*ModuleDOPresent+2+[index]*2 |count: 1| R/W
        /// </summary>
        /// <value>0..65535</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 RouteFrom { get; set; }
        
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count + 5*ModuleDIPresent + 10*ModuleDOPresent+3+[index]*2 |count: 1| R/W
        /// </summary>
        /// <value>0..DeviceHeader.DeviceUserRegistersCount</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 RouteTo { get; set;}

        private bool _routeConfigured;
        public bool RouteConfigured 
        { 
            get { return _routeConfigured; }
            set
            {
                _routeConfigured = value;                
            } 
        }
    }
}
