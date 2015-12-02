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
    }

    public class Device
    {        
        public Device()
        {
            Configuration = new DeviceConfiguration();    
            UserRegisters = new List<DeviceUserRegister>();
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

        #region Configuration Properties

        internal DeviceConfiguration Configuration;                        

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

        public DeviceModuleDI DiscreetInputModule
        {
            get { return Configuration.DIModule; }            
        }

        public DeviceModuleDO DiscreetOutputModule
        {
            get { return Configuration.DOModule; }            
        }

        public bool RoutingEnabled
        {
            get { return Configuration.RoutingHeader.RoutingEnabled != 0; }
            set { Configuration.RoutingHeader.RoutingEnabled = (UInt16)(value ? 1 : 0); }
        }
        
        public List<DeviceRoutingTableElement> RoutingMap
        {
            get { return Configuration.RoutingTable; }
            set { Configuration.RoutingTable = value; }
        }

        public List<List<DeviceModbusMasterQuery>> ModbusMasterPortsQueries
        {
            get { return Configuration.ModbusMasterQueriesOnUartPorts; }
            set { Configuration.ModbusMasterQueriesOnUartPorts = value; }
        }

        #endregion       

        #region Read & Save Methods

        public ReaderSaverErrors SaveConfiguration(IDeviceReaderSaver saver)
        {
            return saver.SaveDeviceConfiguration(Configuration);
        }

        public ReaderSaverErrors ReadConfiguration(IDeviceReaderSaver reader)
        {
            ReaderSaverErrors code = reader.ReadDeviceConfiguration(ref Configuration);
            if (code == ReaderSaverErrors.CodeOk)
            {
                UserRegistersMapBuilder userMapBuilder = new UserRegistersMapBuilder(Configuration);
                userMapBuilder.BuildRegistersMap(ref UserRegisters);
            }
            return code;
        }

        #endregion

        #endregion               

        #region User registers map

        public List<DeviceUserRegister> UserRegisters;

        public ReaderSaverErrors ReadUserRegistersFromDevice(ModbusReaderSaver reader)
        {
            UInt16 oldOffset = reader.RegisterReadAddressOffset;
            reader.RegisterReadAddressOffset = 0;
            ReaderSaverErrors code =  reader.ReadUserRegisters(ref UserRegisters);
            reader.RegisterReadAddressOffset = oldOffset;
            return code;
        }
        #endregion
    }
}
