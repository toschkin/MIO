using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIOConfig;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceUserRegister
    {
        private DeviceConfiguration _deviceConfiguration;        
        public DeviceUserRegister(DeviceConfiguration deviceConfiguration)
        {            
            _deviceConfiguration = deviceConfiguration;     
            Source = new List<object>();
        }

        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 Value { get; set; }

        public UInt16 Address { get; set; }
       
        public List<object> Source { get; set; }

        public string SourceDescription
        {
            get
            {
                if (_deviceConfiguration == null)
                    return "нет данных";

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var source in Source)
                {
                    if (source is DeviceModbusMasterQuery)
                    {
                        for (var port = 0; port < _deviceConfiguration.ModbusMasterQueriesOnUartPorts.Count; port++)
                        {
                            if (_deviceConfiguration.UartPorts[port].PortProtocolType != Definitions.MODBUS_MASTER_PROTOCOL)
                                continue;
                            for (var query = 0; query < _deviceConfiguration.ModbusMasterQueriesOnUartPorts[port].Count; query++)
                            {
                                if(!_deviceConfiguration.ModbusMasterQueriesOnUartPorts[port][query].QueryConfigured)
                                    continue;

                                if (((DeviceModbusMasterQuery)source) == _deviceConfiguration.ModbusMasterQueriesOnUartPorts[port][query])
                                    stringBuilder.AppendLine(String.Format("Источник: Запрос Modbus master\nПорт №:\t{0}\nЗапрос №:\t{1}", port + 1, query + 1));
                            }
                        }
                    }
                    if (source is DeviceRoutingTableElement)
                    {
                        for (var route = 0; route < _deviceConfiguration.RoutingTable.Count; route++)
                        {
                            if (((DeviceRoutingTableElement) source) == _deviceConfiguration.RoutingTable[route] && _deviceConfiguration.RoutingTable[route].RouteConfigured)
                            {
                                stringBuilder.AppendLine(String.Format("Источник: Маршрут таблицы маршрутизаци №\t{0}", route + 1));
                            }
                                
                        }
                    }
                }
                return stringBuilder.ToString();
            }
        }        
    }
}
