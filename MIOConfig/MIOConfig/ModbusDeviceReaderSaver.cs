using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    public class ModbusDeviceReaderSaver : IDeviceReaderSaver
    {
        private ModbusRtuProtocol _protocol;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="registerAddressOffset"></param>
        public ModbusDeviceReaderSaver(ModbusRtuProtocol protocol, Byte slaveAddress = 1, UInt16 registerAddressOffset = 0, bool bigEndianOrder = false)
        {
            _protocol = protocol;
            SlaveAddress = slaveAddress;
            RegisterAddressOffset = registerAddressOffset;
            BigEndianOrder = bigEndianOrder;
        }

        public Byte     SlaveAddress { get; set; }
        public UInt16   RegisterAddressOffset { get; set; }
        public bool     BigEndianOrder { get; set; }

        public bool SaveDeviceConfiguration(List<object> configurationItems)
        {
            if (_protocol.IsConnected)
            {
                if (_protocol.PresetMultipleRegisters(SlaveAddress, RegisterAddressOffset, configurationItems,
                    BigEndianOrder) == ModbusErrorCode.CodeOk)
                {
                    return true;  
                }                                  
            }
            return false;
        }

        public bool ReadDeviceConfiguration(ref List<object> configurationItems)
        {
            if (_protocol.IsConnected)
            {
                if (_protocol.ReadHoldingRegisters(SlaveAddress, RegisterAddressOffset,ref configurationItems,
                    BigEndianOrder) == ModbusErrorCode.CodeOk)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
