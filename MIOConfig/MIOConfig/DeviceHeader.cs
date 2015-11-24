using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceHeader
    {
        private DeviceConfiguration _deviceConfiguration;
        private UInt16 _deviceUartChannelsCount;
        public bool ModuleModbusMaster;
        public bool ModuleRetranslator;
        public bool ModuleTS;
        public bool ModuleTU;
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
            _deviceUartChannelsCount = 1;
        }

        public DeviceHeader(ref DeviceConfiguration deviceConfiguration)
        {
            _deviceConfiguration = deviceConfiguration;
            _deviceUartChannelsCount = 1;
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
                if ((ModuleTS) && (_deviceConfiguration != null))
                    _deviceConfiguration.DeviceDIModule = new DeviceModuleDI();
                else
                    _deviceConfiguration.DeviceDIModule = null;
            }
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

        #region Methods        
        public bool IsValidHeader()
        {
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
            resultString.Append("Состав устройства:\r\n");
            resultString.AppendFormat("\tМодуль Modbus master - {0}\r\n", (ModuleModbusMaster) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль ретрансляции - {0}\r\n", (ModuleRetranslator) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль ТС - {0}\r\n", (ModuleTS) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль ТУ - {0}\r\n", (ModuleTU) ? "есть" : "нет");
            resultString.AppendFormat("\tМодуль Modbus slave - {0}\r\n", (ModuleModbusSlave) ? "есть" : "нет");

            resultString.AppendFormat("Основные параметры:\r\n", DeviceUartChannelsCount);
            resultString.AppendFormat("\tКоличество портов UART: {0}\r\n",DeviceUartChannelsCount);
            resultString.AppendFormat("\tКоличество польз.регистров: {0}\r\n", DeviceUserRegistersCount);
            resultString.AppendFormat("\tМакс кол-во запросов Modbus: {0}\r\n", DeviceMaximumModbusMasterRequestsToSubDeviceCount);
            resultString.AppendFormat("\t[CRC заголовка: {0}]\r\n", DeviceHeaderCrc16);
            return resultString.ToString();
        }
        #endregion
    }
}