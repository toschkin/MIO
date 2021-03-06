﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIOConfig
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
            for (var reg = Definitions.USER_REGISTERS_OFFSET; reg < Definitions.USER_REGISTERS_OFFSET + _configuration.HeaderFields.DeviceUserRegistersCount; reg++)
            {
                DeviceUserRegister register = new DeviceUserRegister(_configuration) {Address = (UInt16) reg};
                if (_configuration.HeaderFields.ModuleRouter)
                {
                    foreach (var route in _configuration.RoutingTable.Where(route => register.Address == route.RouteTo))
                    {
                        register.Source.Add(route);
                    }    
                }
                if (_configuration.HeaderFields.ModuleModbusMaster)
                {
                    //error here
                    foreach (var query in from port in _configuration.ModbusMasterQueriesOnUartPorts
                                          from query in port
                                          where (query.QueryConfigured && (((register.Address >= query.RouteStartAddress)
                                          && (register.Address < query.RouteStartAddress + query.RegistersCount))
                                          || (register.Address == query.QueryStatusAddress)))
                                          select query)
                    {
                        register.Source.Add(query);
                    }  
                }                
                userRegisters.Add(register);
            }            
        }

        public void BuildStatusRegistersMap(ref DeviceStatuses statuses)
        {
            statuses.UartPortStatuses.Clear();
            for (var reg = 0; reg < _configuration.HeaderFields.DeviceUartChannelsCount; reg++)            
                statuses.UartPortStatuses.Add(new DeviceUartPortStatus());                            
        }

        public void BuildDIModuleRegistersMap(ref DeviceDIModule diModule)
        {
            if (_configuration.HeaderFields.ModuleDI && diModule == null)
                diModule = new DeviceDIModule();
            if (_configuration.HeaderFields.ModuleDI == false && diModule != null)
                diModule = null;
        }

        public void BuildDOModuleRegistersMap(ref DeviceDOModule doModule)
        {
            if (_configuration.HeaderFields.ModuleDO && doModule == null)
                doModule = new DeviceDOModule();
            if (_configuration.HeaderFields.ModuleDO == false && doModule != null)
                doModule = null;
        }

        public void BuildRouterModuleRegistersMap(ref DeviceRouterModule routerModule)
        {
            if (_configuration.HeaderFields.ModuleRouter && routerModule == null)
                routerModule = new DeviceRouterModule();
            if (_configuration.HeaderFields.ModuleRouter == false && routerModule != null)
                routerModule = null;
        }
        

        public void BuildAIModuleRegistersMap(ref DeviceAIModule aiModule)
        {
            if (_configuration.HeaderFields.ModuleAI && aiModule == null)
                aiModule = new DeviceAIModule();
            if (_configuration.HeaderFields.ModuleAI == false && aiModule != null)
                aiModule = null;
        }
    }
}
