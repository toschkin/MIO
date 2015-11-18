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
        
        /*public bool SaveDeviceConfiguration(List<object> configurationItems)
        {
            if (_protocol.IsConnected)
            {
                UInt32 registerNumberNeeded = (SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems,
                    ModbusRegisterAccessType.AccessRead) + 1) / 2;
                if ((registerNumberNeeded > 0) &&
                    (registerNumberNeeded <= (UInt16.MaxValue - RegisterAddressOffset) + 1))
                {
                    ModbusDataPoint<byte> poinOfByteSize = new ModbusDataPoint<byte>();
                    UInt16 currentWriteOffset = RegisterAddressOffset;
                    UInt32 totalBytesInQuery = 0;
                    UInt16 currentOffsetInItemsList = 0;
                    int byteInsertedIndex = -1;
                    for (int i = 0; i < configurationItems.Count; i++)
                    {
                        UInt32 bytesNumberNeededForObject =
                            SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems[i],
                                ModbusRegisterAccessType.AccessReadWrite);
                        
                        if (i + 1 < configurationItems.Count)
                        {
                            UInt32 bytesNumberNeededForNextObject = SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems[i+1],
                                ModbusRegisterAccessType.AccessReadWrite);

                            if ((totalBytesInQuery + bytesNumberNeededForObject + bytesNumberNeededForNextObject <= _protocol.WriteRegistersPerQueryCapacity))
                            {
                                totalBytesInQuery += bytesNumberNeededForObject + bytesNumberNeededForNextObject;                                
                                continue;
                            }
                        }                        

                        if (((totalBytesInQuery + bytesNumberNeededForObject + 1) / 2 >= _protocol.WriteRegistersPerQueryCapacity)
                            || ((i == configurationItems.Count - 1)))
                        {
                            

                            if ((totalBytesInQuery + bytesNumberNeededForObject + 1) / 2 == _protocol.WriteRegistersPerQueryCapacity)
                                totalBytesInQuery += bytesNumberNeededForObject;

                            if (_protocol.PresetMultipleRegisters(SlaveAddress, currentWriteOffset, configurationItems, currentOffsetInItemsList, (UInt16)(i + 1 - currentOffsetInItemsList), BigEndianOrder) != ModbusErrorCode.CodeOk)
                                return false;

                            currentOffsetInItemsList += (UInt16)(i + 1 - currentOffsetInItemsList);

                            if (byteInsertedIndex != -1)
                            {
                                configurationItems.RemoveAt(byteInsertedIndex);
                                byteInsertedIndex = -1;
                                currentOffsetInItemsList--;
                                i--;
                            }

                            if (totalBytesInQuery % 2 != 0)
                            {
                                poinOfByteSize.Value =
                                    ModbusDataMappingHelper.GetModbusObjectsHighestByte(
                                        configurationItems[currentOffsetInItemsList - 1],
                                        ModbusRegisterAccessType.AccessReadWrite);
                                configurationItems.Insert(currentOffsetInItemsList, poinOfByteSize);
                                byteInsertedIndex = currentOffsetInItemsList;
                            }

                            if ((bytesNumberNeededForObject + 1) / 2 > _protocol.WriteRegistersPerQueryCapacity)
                            {
                                //TODO big objects (registerNumberNeededForObject > WriteRegistersPerQueryCapacity)
                                //we will skip those enormous objects by now
                                currentWriteOffset += (UInt16)(bytesNumberNeededForObject / 2);
                                if (byteInsertedIndex != -1)
                                {
                                    configurationItems.RemoveAt(byteInsertedIndex);
                                    byteInsertedIndex = -1;
                                }
                                else
                                {
                                    currentOffsetInItemsList++;
                                    i++;
                                }
                            }
                            else
                            {
                                currentWriteOffset += (UInt16)(totalBytesInQuery / 2);
                                totalBytesInQuery = 0;
                            }

                        }
                        else
                        {
                            totalBytesInQuery += bytesNumberNeededForObject;
                        }

                    }
                    return true;
                }                
            }
            return false;
        }*/


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
                        if (_protocol.PresetMultipleRegisters(SlaveAddress, RegisterAddressOffset, configurationItems, 0, (UInt32)configurationItems.Count, BigEndianOrder) != ModbusErrorCode.CodeOk)
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
                    
                    List<object> rawBytesMap = new List<object>();
                    rawBytesMap.AddRange(Array.ConvertAll(forcedValues, b => (object)b));

                    for (UInt16 query = 0; query < (UInt16)((registerNumberNeeded + _protocol.WriteRegistersPerQueryCapacity - 1) / _protocol.WriteRegistersPerQueryCapacity); query++)
                    {
                        UInt32 currentArrayOffset = (UInt16)(query * _protocol.WriteRegistersPerQueryCapacity);
                        if (_protocol.PresetMultipleRegisters(SlaveAddress,
                                                            currentDeviceOffset,
                                                            rawBytesMap,
                                                            (UInt32)(query * _protocol.WriteRegistersPerQueryCapacity),
                                                            rawBytesMap.Count - (currentArrayOffset) > _protocol.WriteRegistersPerQueryCapacity ? _protocol.WriteRegistersPerQueryCapacity : (UInt32)(rawBytesMap.Count - (currentArrayOffset)),
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
                        if (_protocol.ReadHoldingRegisters(SlaveAddress, RegisterAddressOffset, ref configurationItems, 0, (UInt32)configurationItems.Count, BigEndianOrder) != ModbusErrorCode.CodeOk)
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

                    ModbusRtuProtocol.ProcessAnalogData(Array.ConvertAll(rawBytesMap.ToArray(), b => (byte)b),
                        ref tempArray, 0, (UInt16)tempArray.Length, BigEndianOrder);
                    return true;                    
                }                
            }
            return false;
        }
    }
}