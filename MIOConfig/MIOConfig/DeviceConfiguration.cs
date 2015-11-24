using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{    
    [Serializable]
    public class DeviceConfiguration //: IEnumerable
    {
        #region Tracing & Logging
        public delegate void TraceError(string errorMessage);        
        /// <summary>
        /// Exception saving
        /// </summary>
        private TraceError _errorLogger;
        /// <summary>
        /// Adds logger to list of errors saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void TraceError(string errorMessage); </param>
        public void AddErrorLogger(TraceError logger)
        {
            _errorLogger += logger;           
        }
        /// <summary>
        /// Removes logger from current list of errors saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void TraceError(string errorMessage); </param>
        public void RemoveErrorLogger(TraceError logger)
        {
            _errorLogger -= logger;            
        }
        #endregion
        /// <summary>
        /// Default ctor
        /// </summary>
        public DeviceConfiguration()
        {
            var thisDevice = this;
            DeviceHeaderFields = new DeviceHeader(ref thisDevice);                        
            DeviceLastConfigurationTime = new DeviceConfigurationTime();
            //minimum 1 UART presentin device
            DeviceUartPorts = new List<DeviceUARTPortConfiguration>(3) {new DeviceUARTPortConfiguration()};
            DeviceDIModule = null;
        }

        #region Fields & Properties              
        
        /// <summary>
        /// Holding regs|addr.: 1000|count: 5
        /// </summary>          
        public DeviceHeader DeviceHeaderFields;
        
        /// <summary>
        /// Holding regs|addr.: 1005|count: 2| R/W
        /// </summary>        
        public DeviceConfigurationTime DeviceLastConfigurationTime;
       
        /// <summary>
        /// Holding regs|addr.: 1007+7*(DevicePortNumber-1)|count: 7| R/W
        /// </summary>        
        public List<DeviceUARTPortConfiguration> DeviceUartPorts;

        /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count |count: 4| R/W
        /// </summary>     
        public DeviceModuleDI DeviceDIModule;

        #endregion

        #region Methods
    
        private List<object> ToList()
        {
            List<object> listOfConfigurationItems = new List<object>();            
            listOfConfigurationItems.Add(DeviceHeaderFields);
            listOfConfigurationItems.Add(DeviceLastConfigurationTime);
            listOfConfigurationItems.AddRange(DeviceUartPorts);
            if (DeviceDIModule != null)
                listOfConfigurationItems.Add(DeviceDIModule);
            return listOfConfigurationItems;
        }

        private bool SetConfigurationFromList(List<object> listOfConfigurationItems)
        {
            if (listOfConfigurationItems.Count < 3)
                return false;
            int listIndex = 0;
            //Header
            object tempObj = DeviceHeaderFields;
            Utility.CloneObjectProperties(listOfConfigurationItems[listIndex++], ref tempObj);
            DeviceHeaderFields = tempObj as DeviceHeader;            
            //Configuration Time
            DeviceLastConfigurationTime = listOfConfigurationItems[listIndex++] as DeviceConfigurationTime;//Utility.CloneObject(listOfConfigurationItems[listIndex++]) as DeviceConfigurationTime;
            //UART ports
            for (int port = 0; port < DeviceHeaderFields.DeviceUartChannelsCount && listIndex < listOfConfigurationItems.Count; port++)
            {
                DeviceUartPorts[port] = listOfConfigurationItems[listIndex++] as DeviceUARTPortConfiguration;
            }
            if (DeviceHeaderFields.ModuleTS && listIndex < listOfConfigurationItems.Count)
                DeviceDIModule = listOfConfigurationItems[listIndex++] as DeviceModuleDI;

            return true;
        }               

        /// <summary>
        /// Reads header elements (so arrays will be resized) and checks it's validity
        /// </summary>
        /// <param name="reader">interface to configuration reader</param>
        /// <returns>true on success, false - otherwise</returns>
        private bool ReadAndCheckConfigurationHeader(IDeviceReaderSaver reader)
        {            
            List<object> listOfConfigurationItems = new List<object>();            
            listOfConfigurationItems.Add(DeviceHeaderFields);            
            //at first step we will read only header to build all device data map
            if ((reader.ReadDeviceConfiguration(ref listOfConfigurationItems) == false)
                ||(listOfConfigurationItems.Count < 1))
            {
                if (_errorLogger != null)
                    _errorLogger("Ошибка при чтении заголовка конфигурации устройства");
                return false;
            }

            //DeviceHeaderFields = listOfConfigurationItems[0] as DeviceHeader ?? new DeviceHeader();
            object tempObj = DeviceHeaderFields;
            Utility.CloneObjectProperties(listOfConfigurationItems[0], ref tempObj);
            DeviceHeaderFields = tempObj as DeviceHeader;

            if(!DeviceHeaderFields.IsValidHeader())
            {
                if (_errorLogger != null)
                    _errorLogger("Ошибочный заголовок конфигруации устройства/другое устройство");
                return false;
            }
            //SetDeviceUartPortsSize(DeviceHeaderFields.DeviceUartChannelsCount);
            return true;            
        }

        /// <summary>
        /// Saves configuration to device/file etc.
        /// </summary>
        /// <param name="saver">interface to configuration saver</param>
        /// <returns>true on success, false - otherwise</returns>
        public bool SaveConfiguration(IDeviceReaderSaver saver)
        {
            //seting time to moment of configuration
            DeviceLastConfigurationTime.ConfigurationTime = DateTime.Now;

            List<object> listOfConfigurationItems = ToList();
            return saver.SaveDeviceConfiguration(listOfConfigurationItems);
        }

        /// <summary>
        /// Reads configuration from device/file etc.
        /// </summary>
        /// <param name="reader">interface to configuration reader</param>
        /// <returns>true on success, false - otherwise</returns>
        public bool ReadConfiguration(IDeviceReaderSaver reader)
        {
            if(ReadAndCheckConfigurationHeader(reader))
            {
                List<object> listOfConfigurationItems = ToList();
                if (reader.ReadDeviceConfiguration(ref listOfConfigurationItems) == false)
                {
                    if (_errorLogger != null)
                        _errorLogger("Ошибка при чтении конфигурации устройства");
                    return false;
                }
                return SetConfigurationFromList(listOfConfigurationItems);                                            
            }            
            return false;
        }

        public override string ToString()
        {
            StringBuilder resultString = new StringBuilder();
            resultString.Append(DeviceHeaderFields);
            resultString.Append(DeviceLastConfigurationTime);
            return resultString.ToString();
        }

        #endregion
    }
}
