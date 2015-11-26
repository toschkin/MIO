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

    public class Device
    {
        public Device()
        {
            Configuration = new DeviceConfiguration();            
        }

        public override string ToString()
        {
            return Configuration.ToString();
        }

        #region Error Tracing & Logging       
 
        private ErrorTracer _validationMessager;
        /// <summary>
        /// Adds message callback to list 
        /// </summary>
        /// <param name="logger">delegate of form: void ErrorTracer(string errorMessage); </param>
        public void AddValidationMessager(ErrorTracer logger)
        {
            _validationMessager += logger;
        }
        /// <summary>
        /// Removes message callback current list 
        /// </summary>
        /// <param name="logger">delegate of form: void ErrorTracer(string errorMessage); </param>
        public void RemoveValidationMessager(ErrorTracer logger)
        {
            _validationMessager -= logger;
        }

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
        
        public List<DeviceRoutingTableElement> RoutingMap
        {
            get { return Configuration.RoutingTable; }
            set { Configuration.RoutingTable = value; }
        }

        #endregion

        #region Validation Methods

        public bool ValidateRotingMapElement(DeviceRoutingTableElement element)
        {
            if (element.RouteTo > Configuration.HeaderFields.DeviceUserRegistersCount - 1)
            {
                if (_validationMessager != null)
                {
                    _validationMessager(String.Format("Целевой регистр за пределами области пользовательских регистров: 0..{0}", Configuration.HeaderFields.DeviceUserRegistersCount - 1));
                }
                return false;
            }
            for (int route = 0; route < RoutingMap.Count; route++)
            {
                if (element.RouteTo == RoutingMap[route].RouteTo)                    
                {
                    if (_validationMessager != null)
                    {                        
                        _validationMessager(String.Format("Наложение целевого регистра с маршрутом №{0}", route + 1));
                    }
                    return false;
                }                
            }                        
            return true;           
        }
        //TODO 
        /*public bool IsValidUartPortConfiguration(DeviceUARTPortConfiguration uartPort)
        {
            return;
        }*/

        #endregion

        #region Read & Save Methods

        public ReaderSaverErrors SaveConfiguration(IDeviceReaderSaver saver)
        {
            return saver.SaveDeviceConfiguration(this);
        }

        public ReaderSaverErrors ReadConfiguration(IDeviceReaderSaver reader)
        {
            var config = this;
            return reader.ReadDeviceConfiguration(ref config);
        }

        #endregion

        #endregion               
    }
}
