using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceHeader
    {
        private DeviceConfiguration _deviceConfiguration;        
        public bool ModuleModbusMaster;
        public bool ModuleRouter;
        public bool ModuleDI;
        public bool ModuleDO;
        public bool ModuleGPS;
        public bool ModuleMAX7219;
        public bool ModuleAdvantage_TY;
        public bool Reserve7;
        public bool Reserve8;
        public bool Reserve9;
        public bool Reserve10;
        public bool Reserve11;
        public bool Reserve12;
        public bool Reserve13;
        public bool Reserve14;
        public bool ModuleModbusSlave;        
       
        public DeviceHeader()
        {
            _deviceConfiguration = null;                
        }

        public DeviceHeader(ref DeviceConfiguration deviceConfiguration)
        {            
            _deviceConfiguration = deviceConfiguration;           
        } 
        /// <summary>
        /// Holding regs|addr.: 1000|count: 1
        /// </summary>     
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 DeviceConsistenceRegister
        {
            get
            {
                int numeral = 0;
                Byte i = 0;
                foreach (var field in OrderedGetter.GetObjectFieldsInDeclarationOrder(this))
                {
                    if ((bool)field.GetValue(this))
                        numeral |= 1 << i;
                    i++;
                }
                return (UInt16)numeral;
            }
            set
            {
                Byte i = 0;
                foreach (var field in OrderedGetter.GetObjectFieldsInDeclarationOrder(this))
                {
                    field.SetValue(this, (value & (1 << i++)) != 0);
                }
                if (_deviceConfiguration != null)
                {
                    if (ModuleDI)
                    {
                        if (_deviceConfiguration.DIModuleConfiguration == null) 
                            _deviceConfiguration.DIModuleConfiguration = new DeviceModuleDIConfiguration();
                    }
                    else
                        _deviceConfiguration.DIModuleConfiguration = null;

                    if (ModuleDO)
                    {
                        if (_deviceConfiguration.DOModuleConfiguration == null) 
                            _deviceConfiguration.DOModuleConfiguration = new DeviceModuleDOConfiguration();
                    }
                    else
                        _deviceConfiguration.DOModuleConfiguration = null;
                    
                    if (ModuleRouter)
                    {
                        if (_deviceConfiguration.RoutingHeader == null)
                            _deviceConfiguration.RoutingHeader = new DeviceRoutingHeader(ref _deviceConfiguration);
                        if (_deviceConfiguration.RoutingTable == null)
                            _deviceConfiguration.RoutingTable = new ObservableCollection<DeviceRoutingTableElement>();
                    }
                    else
                    {
                        _deviceConfiguration.RoutingTable = null;
                        _deviceConfiguration.RoutingHeader = null;
                    }                   
                }
            }
        }

        private UInt16 _deviceUartChannelsCount;
        /// <summary>
        /// Holding regs|addr.: 1001|count: 1
        /// </summary>             
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]   
        public UInt16 DeviceUartChannelsCount 
        {
            get
            {
                if (_deviceConfiguration != null)
                    return (UInt16)_deviceConfiguration.UartPorts.Count;
                return _deviceUartChannelsCount;
            }
            set
            {
                if (value < 1)
                    _deviceUartChannelsCount = 1;                 

                if (_deviceConfiguration != null)
                {
                    if (value > _deviceConfiguration.UartPorts.Count)
                    {
                        int addCount = value - _deviceConfiguration.UartPorts.Count;

                        for (int i = 0; i < addCount; i++)
                        {
                            _deviceConfiguration.UartPorts.Add(new DeviceUARTPortConfiguration(ref _deviceConfiguration));
                            if (_deviceConfiguration.ModbusMasterQueriesOnUartPorts != null)
                                _deviceConfiguration.ModbusMasterQueriesOnUartPorts.Add(new ObservableCollection<DeviceModbusMasterQuery>());
                        }
                    }

                    if (value < _deviceConfiguration.UartPorts.Count)
                    {
                        _deviceConfiguration.UartPorts.RemoveRange(value, _deviceConfiguration.UartPorts.Count - value); 
                        if (_deviceConfiguration.ModbusMasterQueriesOnUartPorts != null)
                            _deviceConfiguration.ModbusMasterQueriesOnUartPorts.ToList().RemoveRange(value, _deviceConfiguration.UartPorts.Count - value); 
                    }                        
                }
                _deviceUartChannelsCount = value;
            }
        }

        private UInt16 _deviceUserRegistersCount;

        /// <summary>
        /// Holding regs|addr.: 1002|count: 1| R/O
        /// </summary>  
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 DeviceUserRegistersCount
        {
            get { return _deviceUserRegistersCount; }
            set
            {
                _deviceUserRegistersCount = value;
                if (_deviceConfiguration != null && _deviceConfiguration.RoutingTable != null)
                {
                    if (value > _deviceConfiguration.RoutingTable.Count)
                    {
                        int addCount = value - _deviceConfiguration.RoutingTable.Count;
                        for (int i = 0; i < addCount; i++)
                        {
                            _deviceConfiguration.RoutingTable.Add(new DeviceRoutingTableElement());
                        }
                        return;
                    }
                    _deviceConfiguration.RoutingTable.ToList().RemoveRange(value, _deviceConfiguration.RoutingTable.Count - value);
                }
            }
        }

        private UInt16 _deviceMaximumModbusMasterRequestsToSubDeviceCount;
        /// <summary>
        /// Holding regs|addr.: 1003|count: 1| R/O
        /// </summary>  
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 DeviceMaximumModbusMasterRequestsToSubDeviceCount
        {
            get
            {
                if (_deviceConfiguration != null && _deviceConfiguration.ModbusMasterQueriesOnUartPorts != null && _deviceConfiguration.ModbusMasterQueriesOnUartPorts.Count > 0)
                    return (UInt16)_deviceConfiguration.ModbusMasterQueriesOnUartPorts[0].Count;
                return _deviceMaximumModbusMasterRequestsToSubDeviceCount;
                //return 1;
            }
            set
            {
                _deviceMaximumModbusMasterRequestsToSubDeviceCount = value;
                /*if (value < 1)
                    value = 1;*/
                if (_deviceConfiguration == null || _deviceConfiguration.ModbusMasterQueriesOnUartPorts == null) 
                    return;

                if(_deviceConfiguration.ModbusMasterQueriesOnUartPorts.Count == 0)
                    return;
                
                if (value > _deviceConfiguration.ModbusMasterQueriesOnUartPorts[0].Count)
                {
                    int addCount = value - _deviceConfiguration.ModbusMasterQueriesOnUartPorts[0].Count;

                    foreach (var port in _deviceConfiguration.ModbusMasterQueriesOnUartPorts)
                    {
                        for (int i = 0; i < addCount; i++)
                        {
                            port.Add(new DeviceModbusMasterQuery());                                
                        }
                    }                                                
                }
                if (value >= _deviceConfiguration.ModbusMasterQueriesOnUartPorts[0].Count) return;
                foreach (var port in _deviceConfiguration.ModbusMasterQueriesOnUartPorts)
                {
                    port.ToList().RemoveRange(value, _deviceConfiguration.ModbusMasterQueriesOnUartPorts[0].Count - value);
                }
            }
        }

        /// <summary>
        /// Holding regs|addr.: 1004|count: 1| R/O
        /// </summary>  
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 DeviceVersion{get;set;}

        /// <summary>
        /// Holding regs|addr.: 1005|count: 1| R/O
        /// </summary> 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]       
        public UInt16 DeviceHeaderCrc16 { get; set; }

        #region Methods        
        public bool IsValidHeader()
        {            
            List<Byte> byteRepresentationOfHeader = new List<byte>
            {
                BitConverter.GetBytes(DeviceConsistenceRegister).ElementAt(0),
                BitConverter.GetBytes(DeviceConsistenceRegister).ElementAt(1),
                BitConverter.GetBytes(DeviceUartChannelsCount).ElementAt(0),
                BitConverter.GetBytes(DeviceUartChannelsCount).ElementAt(1),
                BitConverter.GetBytes(DeviceUserRegistersCount).ElementAt(0),
                BitConverter.GetBytes(DeviceUserRegistersCount).ElementAt(1),
                BitConverter.GetBytes(DeviceMaximumModbusMasterRequestsToSubDeviceCount).ElementAt(0),
                BitConverter.GetBytes(DeviceMaximumModbusMasterRequestsToSubDeviceCount).ElementAt(1),
                BitConverter.GetBytes(DeviceVersion).ElementAt(0),
                BitConverter.GetBytes(DeviceVersion).ElementAt(1),
                //need to ask gerasimchuk and test
                BitConverter.GetBytes(DeviceHeaderCrc16).ElementAt(1),
                BitConverter.GetBytes(DeviceHeaderCrc16).ElementAt(0)
            };
            //only by now , need to be cleared
            if (Crc16.CheckCrc(byteRepresentationOfHeader.ToArray()))
                return true;
            return false;
        }

        public string DeviceVersionString { get { return DeviceVersion.ToString(); } }

        public override string ToString()
        {
            StringBuilder resultString = new StringBuilder();
            resultString.Append("Состав устройства:\r\n");
            resultString.AppendFormat("\tМодуль Modbus master - {0}\r\n", (ModuleModbusMaster) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль ретрансляции - {0}\r\n", (ModuleRouter) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль ТС - {0}\r\n", (ModuleDI) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль ТУ - {0}\r\n", (ModuleDO) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль Modbus slave - {0}\r\n", (ModuleModbusSlave) ? "есть" : "нет");

            resultString.AppendFormat("Основные параметры:\r\n");
            resultString.AppendFormat("\tВерсия прошивки: {0}\r\n", DeviceVersionString);
            resultString.AppendFormat("\tКоличество портов UART: {0}\r\n",DeviceUartChannelsCount);
            resultString.AppendFormat("\tКоличество польз.регистров: {0}\r\n", DeviceUserRegistersCount);
            resultString.AppendFormat("\tМакс кол-во запросов Modbus: {0}\r\n", DeviceMaximumModbusMasterRequestsToSubDeviceCount);
            resultString.AppendFormat("\t[CRC заголовка: {0}]\r\n", DeviceHeaderCrc16);
            return resultString.ToString();
        }
        #endregion
    }
}