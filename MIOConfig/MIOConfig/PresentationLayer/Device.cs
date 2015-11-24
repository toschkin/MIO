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

        public bool ModuleTSPresent
        {
            get { return Configuration.DeviceHeaderFields.ModuleTS; }            
        }

        public bool ModuleTUPresent
        {
            get { return Configuration.DeviceHeaderFields.ModuleTU; }            
        }

        public bool ModuleModbusMasterPresent
        {
            get { return Configuration.DeviceHeaderFields.ModuleModbusMaster; }            
        }

        public bool ModuleModbusSlavePresent
        {
            get { return Configuration.DeviceHeaderFields.ModuleModbusSlave; }            
        }

        public bool ModuleRetranslatorPresent
        {
            get { return Configuration.DeviceHeaderFields.ModuleRetranslator; }            
        }

        public DateTime ConfigurationTime
        {
            get { return Configuration.DeviceLastConfigurationTime.ConfigurationTime; }
            set { Configuration.DeviceLastConfigurationTime.ConfigurationTime = value; }
        }

        public ReadOnlyCollection<DeviceUARTPortConfiguration> UartPortsConfigurations
        {
            get { return new ReadOnlyCollection<DeviceUARTPortConfiguration>(Configuration.DeviceUartPorts); }            
        }

        public DeviceModuleDI DiscreetInputModule
        {
            get { return Configuration.DeviceDIModule; }            
        }

        public DeviceModuleDO DiscreetOutputModule
        {
            get { return Configuration.DeviceDOModule; }            
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
