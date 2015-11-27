﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MIOConfig.InternalLayer
{       
    [Serializable]
    public class DeviceConfiguration
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
            HeaderFields = new DeviceHeader(ref thisDevice);                        
            LastConfigurationTime = new DeviceConfigurationTime();
            //minimum 1 UART presentin device
            UartPorts = new List<DeviceUARTPortConfiguration>(3) { new DeviceUARTPortConfiguration(ref thisDevice) };
            DIModule = null;
            DOModule = null;
            RoutingHeader = null;
            RoutingTable = null;
            ModbusMasterQueriesOnUartPorts = new List<List<DeviceModbusMasterQuery>>() { new List<DeviceModbusMasterQuery>() };
        }
       
        #region Fields & Properties              
               
        /// <summary>
        /// Holding regs|addr.: 1000|count: 5
        /// </summary>          
        public DeviceHeader HeaderFields;
        
        /// <summary>
        /// Holding regs|addr.: 1005|count: 2| R/W
        /// </summary>        
        public DeviceConfigurationTime LastConfigurationTime;
       
        /// <summary>
        /// Holding regs|addr.: 1007+7*(DevicePortNumber-1)|count: 7| R/W
        /// </summary>        
        public List<DeviceUARTPortConfiguration> UartPorts;

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count |count: 4| R/W
        /// </summary>     
        public DeviceModuleDI DIModule;

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent |count: 4| R/W
        /// </summary>     
        public DeviceModuleDO DOModule;

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count + 5*ModuleDIPresent + 10*ModuleDOPresent |count: 1| R/W
        /// </summary>
        public DeviceRoutingHeader RoutingHeader;

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count + 5*ModuleDIPresent + 10*ModuleDOPresent+2 |count: 2| R/W
        /// </summary>
        public List<DeviceRoutingTableElement> RoutingTable;

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count + 5*ModuleDIPresent + 10*ModuleDOPresent+2 |count: 2| R/W
        /// </summary>
        public List<List<DeviceModbusMasterQuery>> ModbusMasterQueriesOnUartPorts;

        #endregion

        #region Methods

        public List<object> ToList()
        {                  
            List<object> listOfConfigurationItems = new List<object>();            
            listOfConfigurationItems.Add(HeaderFields);
            listOfConfigurationItems.Add(LastConfigurationTime);
            listOfConfigurationItems.AddRange(UartPorts);
            if (DIModule != null)
                listOfConfigurationItems.Add(DIModule);
            if (DOModule != null)
                listOfConfigurationItems.Add(DOModule);
            if (RoutingHeader != null)
                listOfConfigurationItems.Add(RoutingHeader);
            if (RoutingTable != null)
                listOfConfigurationItems.AddRange(RoutingTable);
            foreach (var queriesList in ModbusMasterQueriesOnUartPorts)
            {
                listOfConfigurationItems.AddRange(queriesList);
            }
            return listOfConfigurationItems;
        }

        public bool FromList(List<object> listOfConfigurationItems)
        {
            if (listOfConfigurationItems.Count < 3)
                return false;
            int listIndex = 0;
            //Header
            object tempObj = HeaderFields;
            Utility.CloneObjectProperties(listOfConfigurationItems[listIndex++], ref tempObj);
            HeaderFields = tempObj as DeviceHeader;            
            //Configuration Time
            LastConfigurationTime = listOfConfigurationItems[listIndex++] as DeviceConfigurationTime;//Utility.CloneObject(listOfConfigurationItems[listIndex++]) as DeviceConfigurationTime;
            //UART ports
            for (int port = 0; port < HeaderFields.DeviceUartChannelsCount && listIndex < listOfConfigurationItems.Count; port++)
            {
                UartPorts[port] = listOfConfigurationItems[listIndex++] as DeviceUARTPortConfiguration;
            }

            if (DIModule != null && HeaderFields.ModuleDI && listIndex < listOfConfigurationItems.Count)
                DIModule = listOfConfigurationItems[listIndex++] as DeviceModuleDI;

            if (DOModule != null && HeaderFields.ModuleDO && listIndex < listOfConfigurationItems.Count)
                DOModule = listOfConfigurationItems[listIndex++] as DeviceModuleDO;

            if (RoutingHeader != null && HeaderFields.ModuleRouter && listIndex < listOfConfigurationItems.Count)
            {
                tempObj = RoutingHeader;                
                Utility.CloneObjectProperties(listOfConfigurationItems[listIndex++], ref tempObj);
                RoutingHeader = tempObj as DeviceRoutingHeader;  
            }
                
            if (HeaderFields.ModuleRouter && RoutingTable != null)
            {
                for (int route = 0; route < RoutingHeader.RoutingTableSize && listIndex < listOfConfigurationItems.Count; route++)
                {
                    RoutingTable[route] = listOfConfigurationItems[listIndex++] as DeviceRoutingTableElement;
                }    
            }
            if (HeaderFields.ModuleModbusMaster)
            {
                foreach (List<DeviceModbusMasterQuery> portQueriesList in ModbusMasterQueriesOnUartPorts)
                {
                    for (int query = 0; query < portQueriesList.Count && listIndex < listOfConfigurationItems.Count; query++)
                    {
                        portQueriesList[query] = listOfConfigurationItems[listIndex++] as DeviceModbusMasterQuery;
                    }
                }
            }
            
            return true;
        }
       
        public override string ToString()
        {
            StringBuilder resultString = new StringBuilder();
            resultString.Append(HeaderFields);
            resultString.Append(LastConfigurationTime);
            return resultString.ToString();
        }

        #endregion
    }
}
