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
        private Byte _readRegistersPerQueryCapacity;
        private Byte _writeRegistersPerQueryCapacity;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="registerAddressOffset"></param>
        public ModbusDeviceReaderSaver(ModbusRtuProtocol protocol, Byte slaveAddress = 1, UInt16 registerAddressOffset = 0, bool bigEndianOrder = false, Byte readRegistersPerQueryCapacity = 125,Byte writeRegistersPerQueryCapacity = 125)
        {
            _protocol = protocol;
            SlaveAddress = slaveAddress;
            RegisterAddressOffset = registerAddressOffset;
            BigEndianOrder = bigEndianOrder;
            ReadRegistersPerQueryCapacity = readRegistersPerQueryCapacity;
            WriteRegistersPerQueryCapacity = writeRegistersPerQueryCapacity;
        }

        public Byte     SlaveAddress { get; set; }
        public UInt16   RegisterAddressOffset { get; set; }
        public bool     BigEndianOrder { get; set; }
        public Byte     ReadRegistersPerQueryCapacity 
        {
            get { return _readRegistersPerQueryCapacity; }
            set
            {
                if (value > 125) _readRegistersPerQueryCapacity = 125;
                else _readRegistersPerQueryCapacity = value;
            }
        }
        public Byte     WriteRegistersPerQueryCapacity
         {
            get { return _writeRegistersPerQueryCapacity; }
            set
            {
                if (value > 125) _writeRegistersPerQueryCapacity = 125;
                else _writeRegistersPerQueryCapacity = value;
            }
        }
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
                        if ((totalBytesInQuery + bytesNumberNeededForObject+1)/2 > _readRegistersPerQueryCapacity)
                        {
                            //TODO execute current read query (of size totalBytesInQuery) and start to build a new query
                            if (totalBytesInQuery > 0)
                            {                                
                                if (_protocol.ReadHoldingRegisters(SlaveAddress, currentReadOffset, ref configurationItems, currentOffsetInItemsList, (UInt16)(i - currentOffsetInItemsList), BigEndianOrder) != ModbusErrorCode.CodeOk)
                                    return false;
                                currentOffsetInItemsList += (UInt16)i;
                                if (byteInsertedIndex != -1)
                                    configurationItems.RemoveAt(byteInsertedIndex);
                                if (totalBytesInQuery%2 != 0)
                                {
                                    configurationItems.Insert(currentOffsetInItemsList, new Byte());
                                    byteInsertedIndex = currentOffsetInItemsList;
                                }                                    
                            }
                            
                            if ((bytesNumberNeededForObject + 1)/2 > _readRegistersPerQueryCapacity)
                            {
                                //TODO big objects (registerNumberNeededForObject > _readRegistersPerQueryCapacity)
                                //we will skip those enormous objects by now
                                currentReadOffset += (UInt16)(bytesNumberNeededForObject / 2);
                                currentOffsetInItemsList++;
                            }
                            else
                            {   
                             
                                totalBytesInQuery = bytesNumberNeededForObject;
                                currentReadOffset += (UInt16)(totalBytesInQuery / 2);                                
                            }
                                
                        }
                        else
                        {
                            totalBytesInQuery += bytesNumberNeededForObject;
                        }
                                               
                    }
                   
                    /*UInt32 queriesNeeded = (registerNumberNeeded + _readRegistersPerQueryCapacity - 1) / _readRegistersPerQueryCapacity;
                    for (int i = 0; i < queriesNeeded; i++)
                    {
                        if (_protocol.ReadHoldingRegisters(SlaveAddress, RegisterAddressOffset + i * _readRegistersPerQueryCapacity,
                            ref configurationItems, BigEndianOrder) != ModbusErrorCode.CodeOk)                                
                                    return false;                                
                    }*/
                    return true;
                }                
            }
            return false;
        }
    }
}
