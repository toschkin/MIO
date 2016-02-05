using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIOConfig;

namespace MIOConfig.PresentationLayer
{
    public class DeviceValidator
    {
        private Device _device;
        public DeviceValidator(Device device)
        {
            _device = device;
            ValidationErrorList = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var error in ValidationErrorList)
            {
                stringBuilder.AppendFormat("{0}\n",error);
            }
            return stringBuilder.ToString();
        }

        public List<String> ValidationErrorList { get; set; }

        public void NotifyOnValidation()
        {
            _device.NotifyPropertyChanged("ConfiguredRoutingMapSize");
        }
        #region Validation Methods

        public bool CanAddModbusMasterQuery(DeviceModbusMasterQuery modbusQuery)
        {
            ValidationErrorList.Clear();

            if((modbusQuery.QueryStatusAddress >= modbusQuery.RouteStartAddress) && (modbusQuery.QueryStatusAddress < modbusQuery.RouteStartAddress + modbusQuery.RegistersCount))
            {
                ValidationErrorList.Add("Наложение регистра статуса запроса с самим запросом");
                return false;
            }

            for (int route = 0; route < _device.RoutingMap.Count; route++)
            {
                if ((_device.RoutingMap[route].RouteTo >= modbusQuery.RouteStartAddress)
                    &&
                    (_device.RoutingMap[route].RouteTo < modbusQuery.RouteStartAddress + modbusQuery.RegistersCount))
                {
                    ValidationErrorList.Add(String.Format("Наложение целевых регистра запроса с маршрутом №{0}", route + 1));
                    return false;
                }
            }

            for (int port = 0; port < _device.Configuration.ModbusMasterQueriesOnUartPorts.Count; port++)
            {
                for (int query = 0; query < _device.Configuration.ModbusMasterQueriesOnUartPorts[port].Count; query++)
                {
                    if (_device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].QueryConfigured)
                    {
                        if (Utility.IsIntersect(modbusQuery.RouteStartAddress, modbusQuery.RouteStartAddress + modbusQuery.RegistersCount - 1, _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress, _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress + _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RegistersCount - 1))
                        {
                            ValidationErrorList.Add(String.Format("Наложение целевых регистров запроса с запросом Modbus master: порт № {0}, запрос № {1}", port + 1, query + 1));
                            return false;
                        }
                        if ((modbusQuery.QueryStatusAddress >= _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress) && (modbusQuery.QueryStatusAddress < _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress + _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RegistersCount))
                        {
                            ValidationErrorList.Add(String.Format("Наложение регистра статуса запроса с запросом Modbus master: порт № {0}, запрос № {1}", port + 1, query + 1));
                            return false;
                        }
                    }                    
                }
            }
            return true;
        }

        public bool ValidateRoutingMapElement(DeviceRoutingTableElement element)
        {
            ValidationErrorList.Clear();

            bool retCode = true;
            if (element.RouteTo == element.RouteFrom)
            {
                ValidationErrorList.Add("Целевой регистр совпадает с регистром источником");
                retCode = false;
            }

            if (element.RouteTo < Definitions.USER_REGISTERS_OFFSET || element.RouteTo > Definitions.USER_REGISTERS_OFFSET+_device.Configuration.HeaderFields.DeviceUserRegistersCount - 1)
            {
                ValidationErrorList.Add(String.Format("Целевой регистр за пределами области пользовательских регистров: {0}..{1}", Definitions.USER_REGISTERS_OFFSET, Definitions.USER_REGISTERS_OFFSET+_device.Configuration.HeaderFields.DeviceUserRegistersCount - 1));
                retCode = false;
            }
            for (int route = 0; route < _device.RoutingMap.Count; route++)
            {
                if (element.RouteTo == _device.RoutingMap[route].RouteTo && element != _device.RoutingMap[route])
                {
                    ValidationErrorList.Add(String.Format("Наложение целевого регистра маршрута с маршрутом №{0}", route + 1));
                    retCode = false;
                }

                if (element.RouteFrom == _device.RoutingMap[route].RouteTo && element.RouteTo == _device.RoutingMap[route].RouteFrom && element != _device.RoutingMap[route])
                {
                    ValidationErrorList.Add(String.Format("Зацикливание маршрута с маршрутом №{0}", route + 1));
                    retCode = false;
                }
            }
            for (int port = 0; port < _device.Configuration.ModbusMasterQueriesOnUartPorts.Count; port++)
            {
                if (_device.Configuration.UartPorts[port].PortProtocolType != Definitions.MODBUS_MASTER_PROTOCOL)
                    continue;
                for (int query = 0; query < _device.Configuration.ModbusMasterQueriesOnUartPorts[port].Count; query++)
                {
                    if (_device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].QueryConfigured)
                    {
                        if ((element.RouteTo >=
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress)
                            &&
                            (element.RouteTo <
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress +
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RegistersCount))
                        {
                            ValidationErrorList.Add(
                                String.Format(
                                    "Наложение целевого регистра с запросом Modbus master: порт № {0}, запрос № {1}",
                                    port + 1, query + 1));
                            retCode = false;
                        }
                        if (element.RouteTo ==
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].QueryStatusAddress)
                        {
                            ValidationErrorList.Add(
                                String.Format(
                                    "Наложение целевого регистра со статусом запроса Modbus master: порт № {0}, запрос № {1}",
                                    port + 1, query + 1));
                            retCode = false;
                        }
                    }
                }
            }
            return retCode;
        }

        public bool CanAddRoutingMapElement(DeviceRoutingTableElement element)
        {
            bool retCode = true;
            ValidationErrorList.Clear();

            if (element.RouteTo > _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1)
            {
                ValidationErrorList.Add(String.Format("Целевой регистр за пределами области пользовательских регистров: 0..{0}", _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1));
                retCode = false;
            }
            for (int route = 0; route < _device.RoutingMap.Count; route++)
            {
                if (element.RouteTo == _device.RoutingMap[route].RouteTo)
                {
                    ValidationErrorList.Add(String.Format("Наложение целевого регистра маршрута с маршрутом №{0}", route + 1));
                    retCode = false;
                }
            }
            for (int port = 0; port < _device.Configuration.ModbusMasterQueriesOnUartPorts.Count; port++)
            {
                for (int query = 0; query < _device.Configuration.ModbusMasterQueriesOnUartPorts[port].Count; query++)
                {
                    if (_device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].QueryConfigured)
                    {
                        if ((element.RouteTo >=
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress)
                            &&
                            (element.RouteTo <
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress +
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RegistersCount))
                        {
                            ValidationErrorList.Add(
                                String.Format(
                                    "Наложение целевого регистра с запросом Modbus master: порт № {0}, запрос № {1}",
                                    port + 1, query + 1));
                            retCode = false;
                        }
                        if (element.RouteTo ==
                             _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].QueryStatusAddress)                            
                        {
                            ValidationErrorList.Add(
                                String.Format(
                                    "Наложение целевого регистра со статусом запроса Modbus master: порт № {0}, запрос № {1}",
                                    port + 1, query + 1));
                            retCode = false;
                        }
                    }
                }
            }
            return retCode;
        }

        public bool CanSetUartPortConfiguration(DeviceUARTPortConfiguration uartPort)
        {
            ValidationErrorList.Clear();
            if (uartPort.PortProtocolType == Definitions.MODBUS_MASTER_PROTOCOL &&
                    _device.Configuration.HeaderFields.ModuleModbusMaster == false)
            {
                ValidationErrorList.Add("Протокол Modbus master не поддерживается устройством.");
                return false;
            }
            if (uartPort.PortMasterRequestCount >
                    _device.Configuration.HeaderFields.DeviceMaximumModbusMasterRequestsToSubDeviceCount)
            {
                ValidationErrorList.Add(String.Format("Количество запросов не более {0}", _device.Configuration.HeaderFields.DeviceMaximumModbusMasterRequestsToSubDeviceCount));
                return false;
            }
            for (int port = 0; port < _device.UartPortsConfigurations.Count; port++)
            {
                if (uartPort.PortProtocolType == Definitions.MODBUS_SLAVE_PROTOCOL &&
                    _device.UartPortsConfigurations[port].PortProtocolType == Definitions.MODBUS_SLAVE_PROTOCOL &&
                    _device.UartPortsConfigurations[port].PortModbusAddress != uartPort.PortModbusAddress)
                {
                    ValidationErrorList.Add("Адреса устройства для всех каналов Modbus slave должны быть одинаковыми");
                    return false;
                }
            }
            return true;
        }

        public bool ValidateModbusMasterQuery(DeviceModbusMasterQuery modbusQuery,int portIndex = -1,int queryIndex=-1)
        {

            String portAndQueryInfo = "";
            if(portIndex>=0 && queryIndex>=0)
                portAndQueryInfo = String.Format("[порт {0}, запрос {1}] ", portIndex + 1, queryIndex+1);

            if (_device.Configuration.UartPorts[portIndex].PortProtocolType != Definitions.MODBUS_MASTER_PROTOCOL)
                return true;

            if (modbusQuery.QueryConfigured == false)
                return true;

            ValidationErrorList.Clear();

            bool retCode = true;
           
            if ((modbusQuery.QueryStatusAddress >= modbusQuery.RouteStartAddress) && (modbusQuery.QueryStatusAddress < modbusQuery.RouteStartAddress + modbusQuery.RegistersCount))
            {
                ValidationErrorList.Add(portAndQueryInfo+"Наложение регистра статуса запроса Modbus с самим запросом ");
                retCode = false;
            }

            if (modbusQuery.QueryStatusAddress < Definitions.USER_REGISTERS_OFFSET || modbusQuery.QueryStatusAddress > Definitions.USER_REGISTERS_OFFSET + _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1)
            {
                ValidationErrorList.Add(String.Format(portAndQueryInfo + "Регистр статуса запроса Modbus за пределами области пользовательских регистров: {0}..{1}", Definitions.USER_REGISTERS_OFFSET, Definitions.USER_REGISTERS_OFFSET + _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1));
                retCode = false;
            }

            if (modbusQuery.RouteStartAddress < Definitions.USER_REGISTERS_OFFSET || modbusQuery.RouteStartAddress > Definitions.USER_REGISTERS_OFFSET + _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1)
            {
                ValidationErrorList.Add(String.Format(portAndQueryInfo + "Начало области целевых регистров запроса Modbus выходит за нижний предел области пользовательских регистров: {0}..{1}", Definitions.USER_REGISTERS_OFFSET, Definitions.USER_REGISTERS_OFFSET + _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1));
                retCode = false;
            }

            if (modbusQuery.RouteStartAddress + modbusQuery.RegistersCount - 1 > Definitions.USER_REGISTERS_OFFSET + _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1)
            {
                ValidationErrorList.Add(String.Format(portAndQueryInfo + "Область целевых регистров запроса Modbus выходит за верхний предел области пользовательских регистров: {0}..{1}", Definitions.USER_REGISTERS_OFFSET, Definitions.USER_REGISTERS_OFFSET + _device.Configuration.HeaderFields.DeviceUserRegistersCount - 1));
                retCode = false;
            }

            for (int route = 0; route < _device.RoutingMap.Count; route++)
            {
                if ((_device.RoutingMap[route].RouteTo >= modbusQuery.RouteStartAddress)
                    &&
                    (_device.RoutingMap[route].RouteTo < modbusQuery.RouteStartAddress + modbusQuery.RegistersCount))
                {
                    ValidationErrorList.Add(String.Format(portAndQueryInfo + "Наложение целевых регистров запроса Modbus с маршрутом №{0}", route + 1));
                    retCode = false;
                }
            }

            for (int port = 0; port < _device.Configuration.ModbusMasterQueriesOnUartPorts.Count; port++)
            {
                if (_device.Configuration.UartPorts[port].PortProtocolType != Definitions.MODBUS_MASTER_PROTOCOL)
                    continue;
                for (int query = 0; query < _device.Configuration.ModbusMasterQueriesOnUartPorts[port].Count; query++)
                {
                    if (_device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].QueryConfigured && modbusQuery != _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query])
                    {
                        if (Utility.IsIntersect(modbusQuery.RouteStartAddress, modbusQuery.RouteStartAddress + modbusQuery.RegistersCount - 1, _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress, _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress + _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RegistersCount - 1))
                        {
                            ValidationErrorList.Add(String.Format(portAndQueryInfo + "Наложение целевых регистров запроса с запросом Modbus master: порт № {0}, запрос № {1}", port + 1, query + 1));
                            retCode = false;
                        }
                        if ((modbusQuery.QueryStatusAddress >= _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress) && (modbusQuery.QueryStatusAddress < _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RouteStartAddress + _device.Configuration.ModbusMasterQueriesOnUartPorts[port][query].RegistersCount))
                        {
                            ValidationErrorList.Add(String.Format(portAndQueryInfo + "Наложение регистра статуса запроса с запросом Modbus master: порт № {0}, запрос № {1}", port + 1, query + 1));
                            retCode = false;
                        }
                    }
                }
            }
                        
            return retCode;
        }

        public bool IsFullDeviceValid()
        {            
            foreach (var route in _device.RoutingMap)
            {
                if (route.RouteConfigured)
                {
                    if (!ValidateRoutingMapElement(route))
                        return false;    
                }                
            }
            for (int port = 0; port < _device.ModbusMasterPortsQueries.Count; port++)
            {
                for (int query = 0; query < _device.ModbusMasterPortsQueries[port].Count; query++)
                {
                    if (!ValidateModbusMasterQuery(_device.ModbusMasterPortsQueries[port][query],port,query))
                        return false;    
                }
            }           
            return true;
        }
        #endregion

        
    }
}
