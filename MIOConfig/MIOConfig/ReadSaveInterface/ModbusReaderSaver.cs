using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIOConfig.InternalLayer;
using Modbus.Core;

namespace MIOConfig
{
    public class ModbusReaderSaver : IDeviceReaderSaver
    {
        private IModbus _protocol;        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="registerReadAddressOffset"></param>
        /// <param name="registerWriteAddressOffset"></param>
        public ModbusReaderSaver(IModbus protocol, Byte slaveAddress = 1, UInt16 registerReadAddressOffset = 1000, UInt16 registerWriteAddressOffset = 1005, bool bigEndianOrder = false)
        {
            _protocol = protocol;
            SlaveAddress = slaveAddress;
            RegisterReadAddressOffset = registerReadAddressOffset;
            RegisterWriteAddressOffset = registerWriteAddressOffset;            
            BigEndianOrder = bigEndianOrder;            
        }

        public Byte   SlaveAddress { get; set; }
        public UInt16 RegisterReadAddressOffset { get; set; }
        public UInt16 RegisterWriteAddressOffset { get; set; }
        public bool   BigEndianOrder { get; set; }

        public ReaderSaverErrors SaveDeviceConfiguration(Device configuration)
        {                    
            if (_protocol.IsConnected)
            {
                ReaderSaverErrors retCode = CheckDeviceHeaderValidityAndInitConfiguration(configuration,true);
                if (retCode != ReaderSaverErrors.CodeOk)
                    return retCode;

                configuration.ConfigurationTime = DateTime.Now;
                List<object> configurationItems = configuration.Configuration.ToList();
                configurationItems.RemoveRange(0,1);//remove readonly header
                
                UInt16 registerNumberNeeded = (UInt16)((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems,
                    ModbusRegisterAccessType.AccessReadWrite) + 1) / 2);

                if ((registerNumberNeeded > 0) &&
                    (registerNumberNeeded <= (UInt16.MaxValue - RegisterWriteAddressOffset) + 1))
                {
                    if (registerNumberNeeded <= _protocol.WriteRegistersPerQueryCapacity)
                    {
                        if (_protocol.PresetMultipleRegisters(SlaveAddress, RegisterWriteAddressOffset, configurationItems, BigEndianOrder) != ModbusErrorCode.CodeOk)
                            return ReaderSaverErrors.CodeModbusCommunicationError;
                        return ReaderSaverErrors.CodeOk;
                    }

                    UInt16 currentDeviceOffset = RegisterWriteAddressOffset;
                    ModbusDataPoint<UInt16>[] forcedValues;
                                       
                    object[] tempArray = configurationItems.ToArray();                    
                    Type firstElementType = tempArray[0].GetType();
                                        
                    ModbusDataMappingHelper.ConvertObjectsVaulesToRegisters(tempArray, 0,
                        (UInt32) configurationItems.Count,
                        BigEndianOrder, out forcedValues, firstElementType);
                                               
                    for (UInt16 query = 0; query < (UInt16)((registerNumberNeeded + _protocol.WriteRegistersPerQueryCapacity - 1) / _protocol.WriteRegistersPerQueryCapacity); query++)
                    {
                        UInt32 currentArrayOffset = (UInt16)(query * _protocol.WriteRegistersPerQueryCapacity);
                        if (_protocol.PresetMultipleRegisters(SlaveAddress,
                                                            currentDeviceOffset,
                                                            Array.ConvertAll(forcedValues, b => (object) b),
                                                            (UInt32)(query * _protocol.WriteRegistersPerQueryCapacity),
                                                            forcedValues.Length - (currentArrayOffset) > _protocol.WriteRegistersPerQueryCapacity ? _protocol.WriteRegistersPerQueryCapacity : (UInt32)(forcedValues.Length - (currentArrayOffset)),
                                                            BigEndianOrder) != ModbusErrorCode.CodeOk)
                            return ReaderSaverErrors.CodeModbusCommunicationError;

                        currentDeviceOffset += _protocol.WriteRegistersPerQueryCapacity;
                    }
                    return ReaderSaverErrors.CodeOk;
                }
            }
            return ReaderSaverErrors.CodeComPortNotConnected;
        }

        public ReaderSaverErrors ReadDeviceConfiguration(ref Device configuration)
        {
            ReaderSaverErrors retCode = CheckDeviceHeaderValidityAndInitConfiguration(configuration);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;
            List<object> listOfConfigurationItems = configuration.Configuration.ToList();

            retCode = PerformReading(ref listOfConfigurationItems);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;

            if(! configuration.Configuration.FromList(listOfConfigurationItems))
                return ReaderSaverErrors.CodeInvalidConfigurationSize;

            return retCode;
        }

        private ReaderSaverErrors PerformReading(ref List<object> configurationItems)
        {
            if (_protocol.IsConnected)
            {
                UInt16 registerNumberNeeded = (UInt16)((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(configurationItems,
                    ModbusRegisterAccessType.AccessRead) + 1) / 2);
                if ((registerNumberNeeded > 0) &&
                    (registerNumberNeeded <= (UInt16.MaxValue - RegisterReadAddressOffset) + 1))
                {
                    if (registerNumberNeeded <= _protocol.ReadRegistersPerQueryCapacity)
                    {
                        if (_protocol.ReadHoldingRegisters(SlaveAddress, RegisterReadAddressOffset, ref configurationItems, BigEndianOrder) != ModbusErrorCode.CodeOk)
                            return ReaderSaverErrors.CodeModbusCommunicationError;
                        return ReaderSaverErrors.CodeOk;
                    }

                    UInt16 currentDeviceOffset = RegisterReadAddressOffset;
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
                            return ReaderSaverErrors.CodeModbusCommunicationError;

                        currentDeviceOffset += _protocol.ReadRegistersPerQueryCapacity;
                    }
                    object[] tempArray = configurationItems.ToArray();

                    ModbusDataMappingHelper.ProcessAnalogData(Array.ConvertAll(rawBytesMap.ToArray(), b => (byte)b),
                        ref tempArray, 0, (UInt16)tempArray.Length, BigEndianOrder);
                    return ReaderSaverErrors.CodeOk;
                }
            }
            return ReaderSaverErrors.CodeComPortNotConnected;
        }

        private ReaderSaverErrors CheckDeviceHeaderValidityAndInitConfiguration(Device configuration,bool checkOnly = false)
        {
            List<object> listOfConfigurationItems = new List<object>();
            DeviceHeader tempHeader = new DeviceHeader();
            listOfConfigurationItems.Add(checkOnly ? tempHeader : configuration.Configuration.HeaderFields);


            ReaderSaverErrors retCode = PerformReading(ref listOfConfigurationItems);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;

            if (listOfConfigurationItems.Count < 1)
                return ReaderSaverErrors.CodeUnknownError;

            if (!checkOnly)
            {
                //object tempObj = configuration.Configuration.HeaderFields;
                //Utility.CloneObjectProperties(listOfConfigurationItems[0], ref tempObj);                
                if (!configuration.Configuration.HeaderFields.IsValidHeader())
                    return ReaderSaverErrors.CodeInvalidDeviceHeader;

                /*if (configuration.Configuration.HeaderFields.ModuleModbusMaster && configuration.Configuration.UartPorts != null)
                {
                    //save previous reading  offset
                    UInt16 currentDeviceOffset = RegisterReadAddressOffset;
                    //calculate offset for reading router header
                    listOfConfigurationItems.Add(configuration.Configuration.LastConfigurationTime);
                    listOfConfigurationItems.AddRange(configuration.Configuration.UartPorts);                   

                    RegisterReadAddressOffset = (UInt16)(currentDeviceOffset + ((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(listOfConfigurationItems) + 1) / 2));
                    listOfConfigurationItems.Clear();

                    //read header
                    listOfConfigurationItems.AddRange(configuration.Configuration.UartPorts);
                    retCode = PerformReading(ref listOfConfigurationItems);
                    if (retCode != ReaderSaverErrors.CodeOk)
                        return retCode;
                    //initialize header and routing map
                    //tempObj = configuration.Configuration.RoutingHeader;
                    //Utility.CloneObjectProperties(listOfConfigurationItems[0], ref tempObj);                    
                    //restore previous reading  offset
                    RegisterReadAddressOffset = currentDeviceOffset;
                }*/

                if (configuration.Configuration.HeaderFields.ModuleRouter && configuration.Configuration.RoutingHeader != null)
                {
                    //save previous reading  offset
                    UInt16 currentDeviceOffset = RegisterReadAddressOffset;
                    //calculate offset for reading router header
                    listOfConfigurationItems.Add(configuration.Configuration.LastConfigurationTime);
                    listOfConfigurationItems.AddRange(configuration.Configuration.UartPorts);
                    if (configuration.Configuration.DIModule != null)
                        listOfConfigurationItems.Add(configuration.Configuration.DIModule);
                    if (configuration.Configuration.DOModule != null)
                        listOfConfigurationItems.Add(configuration.Configuration.DOModule);
                                        
                    RegisterReadAddressOffset = (UInt16)(currentDeviceOffset + ((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(listOfConfigurationItems) + 1) / 2));
                    listOfConfigurationItems.Clear();
                   
                    //read header
                    listOfConfigurationItems.Add(configuration.Configuration.RoutingHeader);
                    retCode = PerformReading(ref listOfConfigurationItems);
                    if (retCode != ReaderSaverErrors.CodeOk)
                        return retCode;
                    //initialize header and routing map
                    //tempObj = configuration.Configuration.RoutingHeader;
                    //Utility.CloneObjectProperties(listOfConfigurationItems[0], ref tempObj);                    
                    //restore previous reading  offset
                    RegisterReadAddressOffset = currentDeviceOffset;
                }
            }
            else
            {
                if (!tempHeader.IsValidHeader())
                    return ReaderSaverErrors.CodeInvalidDeviceHeader;

                if (configuration.Configuration.HeaderFields.DeviceUartChannelsCount !=
                    tempHeader.DeviceUartChannelsCount)
                    return ReaderSaverErrors.CodeNotCompliantDevice;                
            }                                   
            return ReaderSaverErrors.CodeOk;
        }
    }
}