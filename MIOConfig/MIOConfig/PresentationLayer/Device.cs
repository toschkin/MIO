using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIOConfig.InternalLayer;

namespace MIOConfig
{   
    public delegate void ErrorTracer(string errorMessage);

    public static class Definitions
    {
        public const Byte MODBUS_MASTER_PRTOCOL = 0;
        public const Byte MODBUS_SLAVE_PRTOCOL = 1;
        public const UInt16 USER_REGISTERS_OFFSET = 0;
        public const UInt16 DEVICE_STATE_OFFSET = 500;
        public const UInt16 CONFIGURATION_READ_OFFSET = 1000;
        public const UInt16 CONFIGURATION_WRITE_OFFSET = 1005;       
    }

    public class Device
    {        
        public Device()
        {
            Configuration = new DeviceConfiguration();    
            UserRegisters = new List<DeviceUserRegister>();
            Statuses = new DeviceStatuses();
            DIModule = null;
        }

        public override string ToString()
        {
            return Configuration.ToString();
        }

        #region Error Tracing & Logging                
        /// <summary>
        /// Exception saving
        /// </summary>       
        /// <summary>
        /// Adds logger to list of errors saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void ErrorTracer(string errorMessage); </param>
        public void AddErrorLogger(ErrorTracer logger)
        {
            Configuration.AddErrorLogger(logger);
        }
        /// <summary>
        /// Removes logger from current list of errors saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void ErrorTracer(string errorMessage); </param>
        public void RemoveErrorLogger(ErrorTracer logger)
        {
            Configuration.RemoveErrorLogger(logger); 
        }

        #endregion

        #region Configuration 

        internal DeviceConfiguration Configuration;    
        
        #region Configuration Properties                            

        public bool ModuleDIPresent
        {
            get { return Configuration.HeaderFields.ModuleDI; }            
        }

        public bool ModuleDOPresent
        {
            get { return Configuration.HeaderFields.ModuleDO; }            
        }

        public bool ModuleModbusMasterPresent
        {
            get { return Configuration.HeaderFields.ModuleModbusMaster; }            
        }

        public bool ModuleModbusSlavePresent
        {
            get { return Configuration.HeaderFields.ModuleModbusSlave; }            
        }

        public bool ModuleRouterPresent
        {
            get { return Configuration.HeaderFields.ModuleRouter; }            
        }

        public DateTime ConfigurationTime
        {
            get { return Configuration.LastConfigurationTime.ConfigurationTime; }
            set { Configuration.LastConfigurationTime.ConfigurationTime = value; }
        }

        //diskuss with Sasha about fixed slave port
        //public ReadOnlyCollection<DeviceUARTPortConfiguration> UartPortsConfigurations
        public List<DeviceUARTPortConfiguration> UartPortsConfigurations
        { 
            //get { return new List<DeviceUARTPortConfiguration>(Configuration.UartPorts); }     
            get { return Configuration.UartPorts; }   
            set { Configuration.UartPorts = value; }   
        }

        public DeviceModuleDIConfiguration DiscreetInputModuleConfiguration
        {
            get { return ModuleDIPresent?Configuration.DIModule:null; }            
        }

        public DeviceModuleDOConfiguration DiscreetOutputModuleConfiguration
        {
            get { return ModuleDOPresent?Configuration.DOModule:null; }            
        }

        public bool RoutingEnabled
        {
            get
            {
                if (Configuration.RoutingHeader != null)
                    return Configuration.RoutingHeader.RoutingEnabled != 0;                
                return false;
            }
            set
            {
                if (Configuration.RoutingHeader != null)
                    Configuration.RoutingHeader.RoutingEnabled = (UInt16)(value ? 1 : 0);
            }
        }
        
        public List<DeviceRoutingTableElement> RoutingMap
        {
            get
            {
                return ModuleRouterPresent?Configuration.RoutingTable:null;
            }
            set
            {
                if (Configuration.RoutingTable != null)
                    Configuration.RoutingTable = value;
            }
        }

        public List<List<DeviceModbusMasterQuery>> ModbusMasterPortsQueries
        {
            get { return Configuration.ModbusMasterQueriesOnUartPorts; }
            set { Configuration.ModbusMasterQueriesOnUartPorts = value; }
        }

        #endregion       

        #region Methods

        public ReaderSaverErrors SaveConfiguration(IDeviceReaderSaver saver)
        {
            return saver.SaveDeviceConfiguration(Configuration);
        }

        public ReaderSaverErrors ReadConfiguration(IDeviceReaderSaver reader)
        {
            ReaderSaverErrors code = reader.ReadDeviceConfiguration(ref Configuration);
            if (code == ReaderSaverErrors.CodeOk)
            {
                RegistersMapBuilder mapBuilder = new RegistersMapBuilder(Configuration);
                mapBuilder.BuildUserRegistersMap(ref UserRegisters);
                mapBuilder.BuildStatusRegistersMap(ref Statuses);
                mapBuilder.BuildDIModuleRegistersMap(ref DIModule);
            }
            return code;
        }

        #endregion

        #endregion               

        #region User registers map

        public List<DeviceUserRegister> UserRegisters;

        #region Methods
        public ReaderSaverErrors ReadUserRegistersFromDevice(ModbusReaderSaver reader)
        {
            UInt16 oldOffset = reader.RegisterReadAddressOffset;
            reader.RegisterReadAddressOffset = Definitions.USER_REGISTERS_OFFSET;
            ReaderSaverErrors code =  reader.ReadUserRegisters(ref UserRegisters);
            reader.RegisterReadAddressOffset = oldOffset;
            return code;
        }
        #endregion
        #endregion

        #region Device Statuses And Control

        internal DeviceStatuses Statuses;

        public ReadOnlyCollection<DeviceUartPortStatus> UartPortStatuses
        {
            get { return new ReadOnlyCollection<DeviceUartPortStatus>(Statuses.UartPortStatuses); }
        }
        //TODO add global status when it will be ready

        #region Methods
        public ReaderSaverErrors ReadSatusRegistersFromDevice(ModbusReaderSaver reader)
        {
            UInt16 oldOffset = reader.RegisterReadAddressOffset;
            reader.RegisterReadAddressOffset = Definitions.DEVICE_STATE_OFFSET;
            ReaderSaverErrors code = reader.ReadStatusRegisters(ref Statuses);
            reader.RegisterReadAddressOffset = oldOffset;
            return code;
        }

        public ReaderSaverErrors RestartDevice(ModbusReaderSaver saver)
        {
            UInt16 oldOffset = saver.RegisterReadAddressOffset;
            saver.RegisterWriteAddressOffset = Definitions.DEVICE_STATE_OFFSET;
            ReaderSaverErrors code = saver.RestartDevice();
            saver.RegisterWriteAddressOffset = oldOffset;
            return code;
        }
        #endregion
        #endregion

        #region Device Discreete Input Module

        internal DeviceDIModule DIModule;

        public DeviceDIModule DiscreeteInputModule
        {
            get { return ModuleDIPresent?DIModule:null; }
        }

        public ReaderSaverErrors ReadDIModuleRegistersFromDevice(ModbusReaderSaver reader)
        {
            if (DIModule == null)
                return ReaderSaverErrors.CodeModuleIsAbsent;
            UInt16 oldOffset = reader.RegisterReadAddressOffset;
            reader.RegisterReadAddressOffset = (UInt16)(Definitions.DEVICE_STATE_OFFSET + Statuses.Size);
            ReaderSaverErrors code = reader.ReadDIModuleRegisters(ref DIModule);
            reader.RegisterReadAddressOffset = oldOffset;
            return code;
        }

        #endregion
    }
}
