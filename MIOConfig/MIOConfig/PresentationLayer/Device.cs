using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIOConfig.InternalLayer;

namespace MIOConfig
{
    public class Device
    {
        public Device()
        {
            Configuration = new DeviceConfiguration();            
        }

        #region Tracing & Logging        
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

        #region Public properties

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

        public ReadOnlyCollection<DeviceUARTPortConfiguration> UartPortsConfigurations
        {
            get { return new ReadOnlyCollection<DeviceUARTPortConfiguration>(Configuration.UartPorts); }            
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

        //TODO make it with adding & modifying validation
        public List<DeviceRoutingTableElement> RoutingMap
        {
            get { return Configuration.RoutingTable; }
            set { Configuration.RoutingTable = value; }
        }

        #endregion

        #region Read & Save Methods

        public ReaderSaverErrors SaveConfiguration(IDeviceReaderSaver saver)
        {
            return saver.SaveDeviceConfiguration(this);            
        }

        public ReaderSaverErrors ReadConfiguration(IDeviceReaderSaver reader)
        {
            var _config = this;
            return reader.ReadDeviceConfiguration(ref _config);
        }
        
        #endregion

        public override string ToString()
        {
            return Configuration.ToString();
        }
    }
}
