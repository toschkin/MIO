using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    [Serializable]
    public class DeviceUARTPortConfiguration
    {
        private DeviceConfiguration _deviceConfiguration;
            
        public DeviceUARTPortConfiguration(ref DeviceConfiguration deviceConfiguration)
        {
            _deviceConfiguration = deviceConfiguration;
            PortOperation = 1;
            PortSpeed = 9600;
            PortStopBits = 2;
            PortParity = 0;
            PortByteSize = 8;
            PortFlowControl = 0;
            PortProtocolType = 1;
            PortMasterTimeout = 200;
            PortMasterRequestCount = 0;
            PortRetriesCount = 5;
            PortModbusAddress = 1;            
            
        }
        private UInt16 _portOperation;
        /// <summary>
        /// Holding regs|addr.: 1007+7*(DevicePortNumber-1) |count: 1| R/W
        /// </summary>
        /// <value>0 - out of operation, 1 - in operation</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)] 
        public UInt16 PortOperation 
        {
            get { return _portOperation; }
            set { _portOperation = value > 1 ? (UInt16)1 : value; }
        }
        
        /// <summary>
        /// Holding regs|addr.: 1008+7*(DevicePortNumber-1) |count: 1| R/W
        /// </summary>
        /// <value>range: 1200..57600</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 PortSpeed { get; set; }

        private Byte _portStopBits;
        /// <summary>
        /// Holding regs|addr.: 1009+7*(DevicePortNumber-1) LoByte|count: 1| R/W
        /// </summary>
        /// <value>1 or 2 stopbits</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortStopBits 
        {
            get { return _portStopBits; }
            set
            {
                if ((value > 2) || (value < 1))
                {
                    _portStopBits = 1;
                }                    
                else
                {
                    _portStopBits = value;
                }
            }
        }

        private Byte _portParity;
        /// <summary>
        /// Holding regs|addr.: 1009+7*(DevicePortNumber-1) HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - No Parity, 1 - Parity EVEN, 2 - Parity ODD</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortParity
        {
            get { return _portParity; }
            set
            {
                if ((value > 2))
                {
                    _portParity = 0;
                }
                else
                {
                    _portParity = value;
                }
            }
        }

        private Byte _portByteSize;
        /// <summary>
        /// Holding regs|addr.: 1010+7*(DevicePortNumber-1) LoByte|count: 1| R/W
        /// </summary>
        /// <value>7 , 8 or 9</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortByteSize
        {
            get { return _portByteSize; }
            set
            {
                if ((value > 9) || (value < 8))
                {
                    _portByteSize = 8;
                }                    
                else
                {
                    _portByteSize = value;
                }
            }
        }
       
        /// <summary>
        /// Holding regs|addr.: 1010+7*(DevicePortNumber-1) HiByte|count: 1| R/W
        /// </summary>
        /// <value>always 0</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortFlowControl { get; set; }

        private Byte _portProtocolType;
        /// <summary>
        /// Holding regs|addr.: 1011+7*(DevicePortNumber-1) LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - Modbus master, 1 - Modbus slave</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortProtocolType
        {
            get { return _portProtocolType; }
            set
            {
                //check if master present
                if (_deviceConfiguration != null && value == 0 && _deviceConfiguration.HeaderFields.ModuleModbusMaster == false)                
                    value = 1;               
                
                _portProtocolType = value > 1 ? (Byte)1 : value;
            }
        }

        /// <summary>
        /// Holding regs|addr.: 1011+7*(DevicePortNumber-1) HiByte & 1012+7*(DevicePortNumber-1) LoByte|count: 1| R/W
        /// </summary>
        /// <value>range: 0..65535 ms</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 PortMasterTimeout { get; set; }

        private Byte _portMasterRequestCount;
        /// <summary>
        /// Holding regs|addr.: 1012+7*(DevicePortNumber-1) HiByte|count: 1| R/W
        /// </summary>
        /// <value>range: 1..DeviceConfiguration.DeviceMaximumModbusMasterRequestsToSubDeviceCount</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortMasterRequestCount 
        {            
            get
            {                
                if (_deviceConfiguration != null)               
                    return  (Byte)_deviceConfiguration.ModbusMasterQueriesOnUartPorts[_deviceConfiguration.UartPorts.IndexOf(this)].Aggregate(0, (current, query) => query.QueryConfigured ? current + 1 : current);
                return _portMasterRequestCount;
            }
            set
            {
                _portMasterRequestCount = value;//only stub for modbus
                /*if (_deviceConfiguration != null)
                {
                    for (int query = 0; query < value; query++)
                    {
                        _deviceConfiguration.ModbusMasterQueriesOnUartPorts[_deviceConfiguration.UartPorts.IndexOf(this)][query].QueryConfigured = true;
                    }
                } */
            }
        }

        /// <summary>
        /// Holding regs|addr.: 1013+7*(DevicePortNumber-1) LoByte|count: 1| R/W
        /// </summary>
        /// <value>range: 0..255</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortRetriesCount { get; set; }

        private Byte _portModbusAddress;
        /// <summary>
        /// Holding regs|addr.: 1013+7*(DevicePortNumber-1) HiByte|count: 1| R/W
        /// </summary>
        /// <value>range: 1..247</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte PortModbusAddress
        {
            get { return _portModbusAddress; }
            set
            {
                if ((value > 247) || (value < 1))
                {
                    _portModbusAddress = 1;
                }
                else
                {
                    _portModbusAddress = value;
                }
            }
        }
    }
}
