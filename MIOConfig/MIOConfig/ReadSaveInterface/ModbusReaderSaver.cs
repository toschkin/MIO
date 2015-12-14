using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
        public ModbusReaderSaver(IModbus protocol, Byte slaveAddress = 1, UInt16 registerReadAddressOffset = Definitions.CONFIGURATION_READ_OFFSET, UInt16 registerWriteAddressOffset = Definitions.CONFIGURATION_WRITE_OFFSET, bool bigEndianOrder = false)
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
                ReaderSaverErrors retCode = CheckDeviceHeaderValidityAndInitConfiguration(configuration.Configuration,true);
                if (retCode != ReaderSaverErrors.CodeOk)
                    return retCode;

                configuration.Configuration.LastConfigurationTime.ConfigurationTime = DateTime.Now;
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

        public ReaderSaverErrors ReadDeviceConfiguration(Device configuration)
        {
            configuration.Configuration.ConfigurationReadFromDevice = false;
            ReaderSaverErrors retCode = CheckDeviceHeaderValidityAndInitConfiguration(configuration.Configuration);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;
            List<object> listOfConfigurationItems = configuration.Configuration.ToList();

            retCode = PerformReading(ref listOfConfigurationItems);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;

            if (!configuration.Configuration.FromList(listOfConfigurationItems))
                return ReaderSaverErrors.CodeInvalidConfigurationSize;

            if (retCode == ReaderSaverErrors.CodeOk)
                configuration.Configuration.ConfigurationReadFromDevice = true;
            return retCode;
        }

        internal ReaderSaverErrors ReadUserRegisters(ref List<DeviceUserRegister> userRegisters)
        {
            List<object> tempList = Array.ConvertAll(userRegisters.ToArray(), o => (object) o).ToList();
            return PerformReading(ref tempList);
        }

        internal ReaderSaverErrors ReadModuleRegisters(IDeviceModule module)
        {
            List<object> listOfConfigurationItems = module.ToList();

            ReaderSaverErrors retCode = PerformReading(ref listOfConfigurationItems);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;

            if (!module.FromList(listOfConfigurationItems))
                return ReaderSaverErrors.CodeInvalidStatusesSize;

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

        internal ReaderSaverErrors ReadDeviceHeader(DeviceConfiguration configuration)
        {
            List<object> listOfConfigurationItems = new List<object>();
            DeviceHeader tempHeader = new DeviceHeader();
            DeviceConfigurationTime tempConfigurationTime = new DeviceConfigurationTime();
            listOfConfigurationItems.Add(tempHeader);
            listOfConfigurationItems.Add(tempConfigurationTime);


            ReaderSaverErrors retCode = PerformReading(ref listOfConfigurationItems);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;

            if (listOfConfigurationItems.Count < 1)
                return ReaderSaverErrors.CodeUnknownError;

            if (!tempHeader.IsValidHeader())
                    return ReaderSaverErrors.CodeInvalidDeviceHeader;

            configuration.HeaderFields.DeviceConsistenceRegister = tempHeader.DeviceConsistenceRegister;
            configuration.HeaderFields.DeviceHeaderCrc16 = tempHeader.DeviceHeaderCrc16;
            configuration.HeaderFields.DeviceMaximumModbusMasterRequestsToSubDeviceCount = tempHeader.DeviceMaximumModbusMasterRequestsToSubDeviceCount;
            configuration.HeaderFields.DeviceUartChannelsCount = tempHeader.DeviceUartChannelsCount;
            configuration.HeaderFields.DeviceUserRegistersCount = tempHeader.DeviceUserRegistersCount;
            configuration.HeaderFields.DeviceVersion = tempHeader.DeviceVersion;
            configuration.LastConfigurationTime = tempConfigurationTime;
                                                      
            return ReaderSaverErrors.CodeOk;
        }

        internal ReaderSaverErrors CheckDeviceHeaderValidityAndInitConfiguration(DeviceConfiguration configuration, bool checkOnly = false)
        {
            List<object> listOfConfigurationItems = new List<object>();
            DeviceHeader tempHeader = new DeviceHeader();
            listOfConfigurationItems.Add(checkOnly ? tempHeader : configuration.HeaderFields);

            ReaderSaverErrors retCode = PerformReading(ref listOfConfigurationItems);
            if (retCode != ReaderSaverErrors.CodeOk)
                return retCode;

            if (listOfConfigurationItems.Count < 1)
                return ReaderSaverErrors.CodeUnknownError;

            if (!checkOnly)
            {
                object tempObj = configuration.HeaderFields;
                Utility.CloneObjectProperties(listOfConfigurationItems[0], ref tempObj);                
                if (!configuration.HeaderFields.IsValidHeader())
                    return ReaderSaverErrors.CodeInvalidDeviceHeader;                

                if (configuration.HeaderFields.ModuleRouter && configuration.RoutingHeader != null)
                {
                    //save previous reading  offset
                    UInt16 currentDeviceOffset = RegisterReadAddressOffset;
                    //calculate offset for reading router header
                    listOfConfigurationItems.Add(configuration.LastConfigurationTime);
                    listOfConfigurationItems.AddRange(configuration.UartPorts);
                    if (configuration.DIModuleConfiguration != null)
                        listOfConfigurationItems.Add(configuration.DIModuleConfiguration);
                    if (configuration.DOModuleConfiguration != null)
                        listOfConfigurationItems.Add(configuration.DOModuleConfiguration);
                                        
                    RegisterReadAddressOffset = (UInt16)(currentDeviceOffset + ((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(listOfConfigurationItems) + 1) / 2));
                    listOfConfigurationItems.Clear();
                   
                    //read header
                    listOfConfigurationItems.Add(configuration.RoutingHeader);
                    retCode = PerformReading(ref listOfConfigurationItems);
                    if (retCode != ReaderSaverErrors.CodeOk)
                        return retCode;                                 
                    //restore previous reading  offset
                    RegisterReadAddressOffset = currentDeviceOffset;
                }
            }
            else
            {
                if (!tempHeader.IsValidHeader())
                    return ReaderSaverErrors.CodeInvalidDeviceHeader;

                if (configuration.HeaderFields.DeviceUartChannelsCount !=
                    tempHeader.DeviceUartChannelsCount)
                    return ReaderSaverErrors.CodeNotCompliantDevice;                
            }                                   
            return ReaderSaverErrors.CodeOk;
        }

        internal ReaderSaverErrors RestartDevice()
        {
            if (_protocol.IsConnected)
            {                
                if (_protocol.PresetSingleRegister(SlaveAddress, RegisterWriteAddressOffset, (UInt16)0x55FF) != ModbusErrorCode.CodeOk)
                            return ReaderSaverErrors.CodeModbusCommunicationError;
                
                Thread.Sleep(800);

                if (_protocol.PresetSingleRegister(SlaveAddress, RegisterWriteAddressOffset, (UInt16)0xFF55) != ModbusErrorCode.CodeOk)
                    return ReaderSaverErrors.CodeModbusCommunicationError;

                return ReaderSaverErrors.CodeOk;
            }
            return ReaderSaverErrors.CodeComPortNotConnected;
        }
    }
}