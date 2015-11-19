using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    public class ModbusDeviceReaderSaver : IDeviceReaderSaver
    {
        private IModbus _protocol;        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="registerAddressOffset"></param>
        public ModbusDeviceReaderSaver(IModbus protocol, Byte slaveAddress = 1, UInt16 registerAddressOffset = 0, bool bigEndianOrder = false)
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
                UInt16 registerNumberNeeded = (UInt16)((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems,
                    ModbusRegisterAccessType.AccessReadWrite) + 1) / 2);

                if ((registerNumberNeeded > 0) &&
                    (registerNumberNeeded <= (UInt16.MaxValue - RegisterAddressOffset) + 1))
                {
                    if (registerNumberNeeded <= _protocol.WriteRegistersPerQueryCapacity)
                    {
                        if (_protocol.PresetMultipleRegisters(SlaveAddress, RegisterAddressOffset, configurationItems, BigEndianOrder) != ModbusErrorCode.CodeOk)
                            return false;                        
                        return true;
                    }

                    UInt16 currentDeviceOffset = RegisterAddressOffset;
                    ushort[] forcedValues;
                    Type firstElementType;
                    object[] tempArray = configurationItems.ToArray();
                    ModbusDataMappingHelper.ConvertObjectsVaulesToRegisters(tempArray, 0,
                        (UInt32) configurationItems.Count,
                        BigEndianOrder, out forcedValues, out firstElementType);
                                               
                    for (UInt16 query = 0; query < (UInt16)((registerNumberNeeded + _protocol.WriteRegistersPerQueryCapacity - 1) / _protocol.WriteRegistersPerQueryCapacity); query++)
                    {
                        UInt32 currentArrayOffset = (UInt16)(query * _protocol.WriteRegistersPerQueryCapacity);
                        if (_protocol.PresetMultipleRegisters(SlaveAddress,
                                                            currentDeviceOffset,
                                                            Array.ConvertAll(forcedValues, b => (object) b),
                                                            (UInt32)(query * _protocol.WriteRegistersPerQueryCapacity),
                                                            forcedValues.Length - (currentArrayOffset) > _protocol.WriteRegistersPerQueryCapacity ? _protocol.WriteRegistersPerQueryCapacity : (UInt32)(forcedValues.Length - (currentArrayOffset)),
                                                            BigEndianOrder) != ModbusErrorCode.CodeOk)
                            return false;

                        currentDeviceOffset += _protocol.WriteRegistersPerQueryCapacity;
                    }                    
                    return true;
                }
            }
            return false;
        }

        public bool ReadDeviceConfiguration(ref List<object> configurationItems)
        {
            if (_protocol.IsConnected)
            {
                UInt16 registerNumberNeeded = (UInt16)((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems,
                    ModbusRegisterAccessType.AccessRead)+1)/2);
                if ((registerNumberNeeded > 0) &&
                    (registerNumberNeeded <= (UInt16.MaxValue - RegisterAddressOffset) + 1))
                {
                    if (registerNumberNeeded <= _protocol.ReadRegistersPerQueryCapacity)
                    {
                        if (_protocol.ReadHoldingRegisters(SlaveAddress, RegisterAddressOffset, ref configurationItems, BigEndianOrder) != ModbusErrorCode.CodeOk)
                            return false;
                        return true;
                    }
                   
                    UInt16 currentDeviceOffset = RegisterAddressOffset;
                    List<object> rawBytesMap = new List<object>();
                    for (int i = 0; i < registerNumberNeeded * 2; i++)
                        rawBytesMap.Add(new Byte());
                   
                    for (UInt16 query = 0; query < (UInt16)((registerNumberNeeded + _protocol.ReadRegistersPerQueryCapacity - 1) / _protocol.ReadRegistersPerQueryCapacity); query++)
                    {
                        UInt32 currentArrayOffset = (UInt16)(query * _protocol.ReadRegistersPerQueryCapacity);
                        if (_protocol.ReadHoldingRegisters(SlaveAddress, 
                                                            currentDeviceOffset, 
                                                            ref rawBytesMap,
                                                            (UInt32)(query * _protocol.ReadRegistersPerQueryCapacity * 2),
                                                            rawBytesMap.Count - (currentArrayOffset * 2) > _protocol.ReadRegistersPerQueryCapacity * 2 ? (UInt32)(_protocol.ReadRegistersPerQueryCapacity * 2) : (UInt32)(rawBytesMap.Count - (currentArrayOffset * 2)), 
                                                            BigEndianOrder) != ModbusErrorCode.CodeOk)
                            return false;
                        
                        currentDeviceOffset += _protocol.ReadRegistersPerQueryCapacity;
                    }
                    object[] tempArray = configurationItems.ToArray();

                    ModbusDataMappingHelper.ProcessAnalogData(Array.ConvertAll(rawBytesMap.ToArray(), b => (byte)b),
                        ref tempArray, 0, (UInt16)tempArray.Length, BigEndianOrder);
                    return true;                    
                }                
            }
            return false;
        }
    }
}