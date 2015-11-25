using System;
using System.Collections.Generic;
using System.Text;

namespace MIOConfig.InternalLayer
{
    public delegate void ErrorTracer(string errorMessage);   

    [Serializable]
    internal class DeviceConfiguration
    {
        #region Tracing & Logging             
        /// <summary>
        /// Exception saving
        /// </summary>
        private ErrorTracer _errorTracerLogger;
        /// <summary>
        /// Adds logger to list of errors saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void ErrorTracer(string errorMessage); </param>
        public void AddErrorLogger(ErrorTracer logger)
        {
            _errorTracerLogger += logger;           
        }
        /// <summary>
        /// Removes logger from current list of errors saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void ErrorTracer(string errorMessage); </param>
        public void RemoveErrorLogger(ErrorTracer logger)
        {
            _errorTracerLogger -= logger;            
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
            DeviceDOModule = null;
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

         /// <summary>
        /// Holding regs|addr.: 1007+7*DeviceUartPorts.Count+5 |count: 4| R/W
        /// </summary>     
        public DeviceModuleDO DeviceDOModule;
        
        
        #endregion

        #region Methods

        public List<object> ToList()
        {
            List<object> listOfConfigurationItems = new List<object>();            
            listOfConfigurationItems.Add(DeviceHeaderFields);
            listOfConfigurationItems.Add(DeviceLastConfigurationTime);
            listOfConfigurationItems.AddRange(DeviceUartPorts);
            if (DeviceDIModule != null)
                listOfConfigurationItems.Add(DeviceDIModule);
            if (DeviceDOModule != null)
                listOfConfigurationItems.Add(DeviceDOModule);
            return listOfConfigurationItems;
        }

        public bool FromList(List<object> listOfConfigurationItems)
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

            if (DeviceHeaderFields.ModuleDI && listIndex < listOfConfigurationItems.Count)
                DeviceDIModule = listOfConfigurationItems[listIndex++] as DeviceModuleDI;

            if (DeviceHeaderFields.ModuleDO && listIndex < listOfConfigurationItems.Count)
                DeviceDOModule = listOfConfigurationItems[listIndex++] as DeviceModuleDO;

            return true;
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
