﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIOConfig;
using MIOConfig.PresentationLayer;

namespace MIOConfig
{     
    [Serializable]
    public class Device : INotifyPropertyChanged
    {        
        public Device()
        {
            Configuration = new DeviceConfiguration();    
            UserRegisters = new List<DeviceUserRegister>();
            Statuses = new DeviceStatuses();
            DIModule = null;
            DOModule = null;
            AIModule = null;
            RouterModule = null;
        }
        
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public UInt16 UserRegistersStartAddressOffset { get { return Definitions.USER_REGISTERS_OFFSET; } }

        public UInt16 UserRegistersEndAddressOffset
        {
            get
            {
                return (UInt16)(Definitions.USER_REGISTERS_OFFSET + Configuration.HeaderFields.DeviceUserRegistersCount - 1);
            }
        }

        public bool ConfigurationReadFromDevice
        {
            get { return Configuration.ConfigurationReadFromDevice; }            
        }
        public Byte ModbusAddress { get; set; }

        public string ShortDescription
        {
            get
            {
                StringBuilder resultString = new StringBuilder();               
                resultString.AppendFormat("Адрес: {0} (Ver.{1})", ModbusAddress, Configuration.HeaderFields.DeviceVersionString);
                if (Configuration.HeaderFields.ModuleDI)
                    resultString.Append("\nМодуль ТС");
                if (Configuration.HeaderFields.ModuleDO)
                    resultString.Append("\nМодуль ТУ");
                if (Configuration.HeaderFields.ModuleAI)
                    resultString.Append("\nМодуль ТИ");
                if (Configuration.HeaderFields.ModuleModbusMaster)
                    resultString.Append("\nModbus(master)");                                
                if (Configuration.HeaderFields.ModuleRouter)
                    resultString.Append("\nРетранслятор");
                return resultString.ToString();
            }
        }

        public string FullDescription
        {
            get
            {
                return ToString();                
            }
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
        public bool ModuleAIPresent
        {
            get { return Configuration.HeaderFields.ModuleAI; }
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

        public UInt16 UserRegistersCount { get { return Configuration.HeaderFields.DeviceUserRegistersCount; }}
        
        

        public DateTime ConfigurationTime
        {
            get { return Configuration.LastConfigurationTime.ConfigurationTime; }
            set
            {
                Configuration.LastConfigurationTime.ConfigurationTime = value;
                NotifyPropertyChanged("FullDescription");
                NotifyPropertyChanged("ShortDescription");
            }
        }

        
        //public ReadOnlyCollection<DeviceUARTPortConfiguration> UartPortsConfigurations
        public ObservableCollection<DeviceUARTPortConfiguration> UartPortsConfigurations
        { 
            //get { return new List<DeviceUARTPortConfiguration>(Configuration.UartPorts); }     
            get { return new ObservableCollection<DeviceUARTPortConfiguration>(Configuration.UartPorts); }   
            //set { Configuration.UartPorts = value; }   
        }

        public DeviceModuleDIConfiguration DiscreetInputModuleConfiguration
        {
            get { return ModuleDIPresent?Configuration.DIModuleConfiguration:null; }            
        }

        public DeviceModuleDOConfiguration DiscreetOutputModuleConfiguration
        {
            get { return ModuleDOPresent?Configuration.DOModuleConfiguration:null; }            
        }

        public DeviceModuleAIConfiguration AnalogInputModuleConfiguration
        {
            get { return ModuleAIPresent ? Configuration.AIModuleConfiguration : null; }
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

        public UInt16 ConfiguredRoutingMapSize
        {
            get
            {
                if (Configuration.RoutingHeader != null)
                    return Configuration.RoutingHeader.RoutingTableSize;
                return 0;
            }
            set
            {
                if (Configuration.RoutingHeader != null)
                    Configuration.RoutingHeader.RoutingTableSize = value;
            }
        }
        public ObservableCollection<DeviceRoutingTableElement> RoutingMap
        {
            get
            {
                return ModuleRouterPresent?Configuration.RoutingTable: null;
            }            
        }

        public ObservableCollection<ObservableCollection<DeviceModbusMasterQuery>> ModbusMasterPortsQueries
        {
            get
            {
                return Configuration.ModbusMasterQueriesOnUartPorts;
                
            }
            set
            {
                Configuration.ModbusMasterQueriesOnUartPorts = value;
            }
        }

        #endregion       

        #region Methods

        public ReaderSaverErrors SaveConfiguration(IDeviceReaderSaver saver)
        {
            return saver.SaveDeviceConfiguration(this);
        }

        public ReaderSaverErrors ReadConfiguration(IDeviceReaderSaver reader)
        {          
            ReaderSaverErrors code = reader.ReadDeviceConfiguration(this);
            if (code == ReaderSaverErrors.CodeOk)
            {
                RegistersMapBuilder mapBuilder = new RegistersMapBuilder(Configuration);
                mapBuilder.BuildUserRegistersMap(ref UserRegisters);
                mapBuilder.BuildStatusRegistersMap(ref Statuses);
                mapBuilder.BuildDIModuleRegistersMap(ref DIModule);
                mapBuilder.BuildDOModuleRegistersMap(ref DOModule);
                mapBuilder.BuildRouterModuleRegistersMap(ref RouterModule);                
                mapBuilder.BuildAIModuleRegistersMap(ref AIModule);
                NotifyPropertyChanged("FullDescription");
                NotifyPropertyChanged("ShortDescription");
            }
            return code;
        }

        #endregion

        #endregion               

        #region User registers map
        
        [NonSerialized]
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
        
        [NonSerialized]
        internal DeviceStatuses Statuses;
                
        public ReadOnlyCollection<DeviceUartPortStatus> UartPortStatuses
        {
            get { return new ReadOnlyCollection<DeviceUartPortStatus>(Statuses.UartPortStatuses); }
        }        

        #region Methods
        public ReaderSaverErrors ReadSatusRegistersFromDevice(ModbusReaderSaver reader)
        {
            UInt16 oldOffset = reader.RegisterReadAddressOffset;
            reader.RegisterReadAddressOffset = Definitions.DEVICE_STATE_OFFSET;
            ReaderSaverErrors code = reader.ReadModuleRegisters(Statuses);
            reader.RegisterReadAddressOffset = oldOffset;
            return code;
        }

        public ReaderSaverErrors RestartDevice(ModbusReaderSaver saver)
        {
            UInt16 oldOffset = saver.RegisterWriteAddressOffset;
            saver.RegisterWriteAddressOffset = Definitions.DEVICE_STATE_OFFSET;
            ReaderSaverErrors code = saver.RestartDevice();
            saver.RegisterWriteAddressOffset = oldOffset;
            return code;
        }
        #endregion
        #endregion

        #region Device Discreete Input Module

        [NonSerialized]
        internal DeviceDIModule DIModule;
        
        public DeviceDIModule DiscreeteInputModule
        {
            get { return ModuleDIPresent ? DIModule:null; }
        }
       
        #endregion
        
        #region Device Discreete Output Module

        [NonSerialized]
        internal DeviceDOModule DOModule;

        public DeviceDOModule DiscreeteOutputModule
        {
            get { return ModuleDOPresent ? DOModule : null; }
        }

        #endregion

        #region Device Router Module

        [NonSerialized]
        internal DeviceRouterModule RouterModule;

        public DeviceRouterModule RegistersRouterModule
        {
            get { return ModuleRouterPresent ? RouterModule : null; }
        }        

        #endregion

        #region Device Analog Input Module

        [NonSerialized]
        internal DeviceAIModule AIModule;

        public DeviceAIModule AnalogInputModule
        {
            get { return ModuleAIPresent ? AIModule : null; }
        }        

        #endregion
    }
}
