using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace MIOConfig.InternalLayer
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
                foreach (var field in GetType().GetFields())
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
                foreach (var field in GetType().GetFields())
                {
                    field.SetValue(this, (value & (1 << i++)) != 0);
                }
                if (_deviceConfiguration != null)
                {
                    if (ModuleDI)
                    {
                        if (_deviceConfiguration.DIModuleConfiguration == null) _deviceConfiguration.DIModuleConfiguration = new DeviceModuleDIConfiguration();
                    }
                    else
                        _deviceConfiguration.DIModuleConfiguration = null;

                    if (ModuleDO)
                    {
                        if (_deviceConfiguration.DOModuleConfiguration == null) _deviceConfiguration.DOModuleConfiguration = new DeviceModuleDOConfiguration();
                    }
                    else
                        _deviceConfiguration.DOModuleConfiguration = null;
                    
                    if (ModuleRouter)
                    {
                        if (_deviceConfiguration.RoutingHeader == null)
                            _deviceConfiguration.RoutingHeader = new DeviceRoutingHeader(ref _deviceConfiguration);
                        if (_deviceConfiguration.RoutingTable == null)
                            _deviceConfiguration.RoutingTable = new List<DeviceRoutingTableElement>();
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
                                _deviceConfiguration.ModbusMasterQueriesOnUartPorts.Add(new List<DeviceModbusMasterQuery>());
                        }
                    }

                    if (value < _deviceConfiguration.UartPorts.Count)
                    {
                        _deviceConfiguration.UartPorts.RemoveRange(value, _deviceConfiguration.UartPorts.Count - value); 
                        if (_deviceConfiguration.ModbusMasterQueriesOnUartPorts != null)
                            _deviceConfiguration.ModbusMasterQueriesOnUartPorts.RemoveRange(value, _deviceConfiguration.UartPorts.Count - value); 
                    }                        
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
        public UInt16 DeviceMaximumModbusMasterRequestsToSubDeviceCount
        {
            get
            {
                if (_deviceConfiguration != null && _deviceConfiguration.ModbusMasterQueriesOnUartPorts != null && _deviceConfiguration.ModbusMasterQueriesOnUartPorts.Count > 0)
                    return (UInt16)_deviceConfiguration.ModbusMasterQueriesOnUartPorts[0].Count;
                return 1;
            }
            set
            {
                if (value < 1)
                    value = 1;
                if (_deviceConfiguration == null || _deviceConfiguration.ModbusMasterQueriesOnUartPorts == null ||
                    _deviceConfiguration.ModbusMasterQueriesOnUartPorts.Count <= 0) return;
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
                    port.RemoveRange(value, _deviceConfiguration.UartPorts.Count - value);
                }
            }
        }

        /// <summary>
        /// Holding regs|addr.: 1004|count: 1| R/O
        /// </summary> 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]       
        public UInt16 DeviceHeaderCrc16 { get; set; }

        #region Methods        
        public bool IsValidHeader()
        {
            //by now 
            return true;
            List<Byte> byteRepresentationOfHeader = new List<byte>
            {
                BitConverter.GetBytes(DeviceConsistenceRegister).ElementAt(1),
                BitConverter.GetBytes(DeviceConsistenceRegister).ElementAt(0),
                BitConverter.GetBytes(DeviceUartChannelsCount).ElementAt(1),
                BitConverter.GetBytes(DeviceUartChannelsCount).ElementAt(0),
                BitConverter.GetBytes(DeviceUserRegistersCount).ElementAt(1),
                BitConverter.GetBytes(DeviceUserRegistersCount).ElementAt(0),
                BitConverter.GetBytes(DeviceMaximumModbusMasterRequestsToSubDeviceCount).ElementAt(1),
                BitConverter.GetBytes(DeviceMaximumModbusMasterRequestsToSubDeviceCount).ElementAt(0),
                //need to ask gerasimchuk and test
                BitConverter.GetBytes(DeviceHeaderCrc16).ElementAt(0),
                BitConverter.GetBytes(DeviceHeaderCrc16).ElementAt(1)
            };
            //only by now , need to be cleared
            if (Crc16.CheckCrc(byteRepresentationOfHeader.ToArray()))
                return true;
            return false;
        }
        public override string ToString()
        {
            StringBuilder resultString = new StringBuilder();
            resultString.Append("������ ����������:\r\n");
            resultString.AppendFormat("\t������ Modbus master - {0}\r\n", (ModuleModbusMaster) ? "����" : "���");
            resultString.AppendFormat("\t������ ������������ - {0}\r\n", (ModuleRouter) ? "����" : "���");
            resultString.AppendFormat("\t������ �� - {0}\r\n", (ModuleDI) ? "����" : "���");
            resultString.AppendFormat("\t������ �� - {0}\r\n", (ModuleDO) ? "����" : "���");
            resultString.AppendFormat("\t������ Modbus slave - {0}\r\n", (ModuleModbusSlave) ? "����" : "���");

            resultString.AppendFormat("�������� ���������:\r\n", DeviceUartChannelsCount);
            resultString.AppendFormat("\t���������� ������ UART: {0}\r\n",DeviceUartChannelsCount);
            resultString.AppendFormat("\t���������� �����.���������: {0}\r\n", DeviceUserRegistersCount);
            resultString.AppendFormat("\t���� ���-�� �������� Modbus: {0}\r\n", DeviceMaximumModbusMasterRequestsToSubDeviceCount);
            resultString.AppendFormat("\t[CRC ���������: {0}]\r\n", DeviceHeaderCrc16);
            return resultString.ToString();
        }
        #endregion
    }
}