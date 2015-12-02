using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    public class DeviceDIModule
    {
        public DeviceDIModule()
        {
            ModuleStatus = new DeviceDIModuleStatus();
            InputsRegisters = new List<ModbusDataPoint<UInt16>>(8) { new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>() };
        }
        public DeviceDIModuleStatus ModuleStatus { get; set; }

        public List<ModbusDataPoint<UInt16>> InputsRegisters;

        public List<bool> InputsStates
        {
            get
            {
                return InputsRegisters.Select(register => register.Value > 0).ToList();
            }
        }

        public List<object> ToList()
        {
            List<object> listOfConfigurationItems = new List<object>();
            listOfConfigurationItems.Add(ModuleStatus);
            listOfConfigurationItems.AddRange(InputsRegisters);
            return listOfConfigurationItems;
        }

        public bool FromList(List<object> listOfConfigurationItems)
        {
            if (listOfConfigurationItems.Count < 2)
                return false;
            int listIndex = 0;
            //Header
            object tempObj = ModuleStatus;
            Utility.CloneObjectProperties(listOfConfigurationItems[listIndex++], ref tempObj);
            ModuleStatus = tempObj as DeviceDIModuleStatus;
            //UART ports
            for (int reg = 0; reg < InputsRegisters.Count && listIndex < listOfConfigurationItems.Count; reg++)
            {
                InputsRegisters[reg] = listOfConfigurationItems[listIndex++] as ModbusDataPoint<UInt16>;
            }
            return true;
        }
    }
}
