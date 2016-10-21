using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceRouterModule : IDeviceModule
    {
        public DeviceRouterModule()
        {
            ModuleStatus = new ModbusDataPoint<UInt16>();           
        }


        public ModbusDataPoint<UInt16> ModuleStatus { get; set; }
        public List<object> ToList()
        {
            List<object> listOfConfigurationItems = new List<object>();
            listOfConfigurationItems.Add(ModuleStatus);            
            return listOfConfigurationItems;
        }

        public bool FromList(List<object> listOfConfigurationItems)
        {
            if (listOfConfigurationItems.Count < 1)
                return false;
            int listIndex = 0;
            //Header                       
            ModuleStatus = listOfConfigurationItems[listIndex++] as ModbusDataPoint<UInt16> ?? new ModbusDataPoint<UInt16>();            
            return true;
        }
        public UInt16 Size
        {
            get
            {
                return (UInt16)(SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(ModuleStatus));
            }
        }
    }
}
