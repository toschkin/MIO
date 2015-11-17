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
                UInt32 registerNumberNeeded = (SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems,
                    ModbusRegisterAccessType.AccessRead) + 1) / 2;
                if ((registerNumberNeeded > 0) &&
                    (registerNumberNeeded <= (UInt16.MaxValue - RegisterAddressOffset) + 1))
                {
                    UInt16 currentWriteOffset = RegisterAddressOffset;
                    UInt32 totalBytesInQuery = 0;
                    UInt16 currentOffsetInItemsList = 0;
                    int byteInsertedIndex = -1;
                    for (int i = 0; i < configurationItems.Count; i++)
                    {
                        UInt32 bytesNumberNeededForObject =
                            SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems[i],
                                ModbusRegisterAccessType.AccessRead);
                        if (((totalBytesInQuery + bytesNumberNeededForObject + 1) / 2 >= _protocol.WriteRegistersPerQueryCapacity)
                            || (i == configurationItems.Count - 1))
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
                                //!!!!!
                                //configurationItems.Insert(currentOffsetInItemsList, ModbusDataMappingHelper.GetObjectsHighestByte(configurationItems[currentOffsetInItemsList - 1]));
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
                /*if (_protocol.PresetMultipleRegisters(SlaveAddress, RegisterAddressOffset, configurationItems,
                    BigEndianOrder) == ModbusErrorCode.CodeOk)
                {
                    return true;  
                }*/
            }
            return false;
        }

        public bool ReadDeviceConfiguration(ref List<object> configurationItems)
        {
            if (_protocol.IsConnected)
            {
                UInt32 registerNumberNeeded = (SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems,
                    ModbusRegisterAccessType.AccessRead)+1)/2;
                if ((registerNumberNeeded > 0) && (registerNumberNeeded <= (UInt16.MaxValue - RegisterAddressOffset) + 1))
                {                    
                    UInt16 currentReadOffset = RegisterAddressOffset;
                    UInt32 totalBytesInQuery = 0;
                    UInt16 currentOffsetInItemsList = 0;
                    int  byteInsertedIndex = -1;
                    for (int i = 0; i < configurationItems.Count; i++)
                    {                                                           
                        UInt32 bytesNumberNeededForObject =
                            SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems[i],
                                ModbusRegisterAccessType.AccessRead);
                        if (((totalBytesInQuery + bytesNumberNeededForObject + 1) / 2 >= _protocol.ReadRegistersPerQueryCapacity)
                            ||(i == configurationItems.Count-1))
                        {
                            if ((totalBytesInQuery + bytesNumberNeededForObject + 1) / 2 == _protocol.ReadRegistersPerQueryCapacity)
                                totalBytesInQuery += bytesNumberNeededForObject;
                                                          
                            if (_protocol.ReadHoldingRegisters(SlaveAddress, currentReadOffset, ref configurationItems, currentOffsetInItemsList, (UInt16)(i+1 - currentOffsetInItemsList), BigEndianOrder) != ModbusErrorCode.CodeOk)
                                return false;
                                
                            currentOffsetInItemsList += (UInt16)(i + 1 - currentOffsetInItemsList);
                                
                            if (byteInsertedIndex != -1)
                            {
                                configurationItems.RemoveAt(byteInsertedIndex);
                                byteInsertedIndex = -1;
                                currentOffsetInItemsList--;                                    
                                i--;                                    
                            }
                                    
                            if (totalBytesInQuery%2 != 0)
                            {
                                configurationItems.Insert(currentOffsetInItemsList, new ModbusDataPoint<byte>());
                                byteInsertedIndex = currentOffsetInItemsList;                                                                   
                            }

                            if ((bytesNumberNeededForObject + 1) / 2 > _protocol.ReadRegistersPerQueryCapacity)
                            {
                                //TODO big objects (registerNumberNeededForObject > _readRegistersPerQueryCapacity)
                                //we will skip those enormous objects by now
                                currentReadOffset += (UInt16)(bytesNumberNeededForObject / 2);                                
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
                                currentReadOffset += (UInt16)(totalBytesInQuery / 2);
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
        }
    }
}
