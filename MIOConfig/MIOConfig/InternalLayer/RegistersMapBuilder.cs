using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIOConfig.InternalLayer
{
    internal class RegistersMapBuilder
    {
        
        private DeviceConfiguration _configuration;

        public RegistersMapBuilder(DeviceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void BuildUserRegistersMap(ref List<DeviceUserRegister> userRegisters)
        {
            userRegisters.Clear();
            for (var reg = 0; reg < _configuration.HeaderFields.DeviceUserRegistersCount; reg++)
            {
                DeviceUserRegister register = new DeviceUserRegister(_configuration) {Address = (UInt16) reg};
                
                foreach (var route in _configuration.RoutingTable.Where(route => register.Address == route.RouteTo))
                {
                    register.Source.Add(route);
                }
                foreach (var query in from port in _configuration.ModbusMasterQueriesOnUartPorts
                                      from query in port
                                      where (query.QueryConfigured && (((register.Address >= query.RouteStartAddress)                                                                                                                           
                                      &&(register.Address < query.RouteStartAddress + query.RegistersCount))                                                                                                                          
                                      ||(register.Address == query.QueryStatusAddress))) select query)
                {
                    register.Source.Add(query);
                }
                userRegisters.Add(register);
            }            
        }

        public void BuildStatusRegistersMap(ref DeviceStatuses statuses)
        {
            statuses.UartPortStatuses.Clear();
            for (var reg = 0; reg < _configuration.HeaderFields.DeviceUartChannelsCount; reg++)            
                statuses.UartPortStatuses.Add(new UARTPortStatus());                            
        }
    }
}
