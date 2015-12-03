using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    public class DeviceDIModule : IDeviceModule
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
            if (listOfConfigurationItems.Count < Size)
                return false;
            int listIndex = 0;
            //Header           
            ModuleStatus = listOfConfigurationItems[listIndex++] as DeviceDIModuleStatus ?? new DeviceDIModuleStatus();
            //UART ports
            for (int reg = 0; reg < InputsRegisters.Count && listIndex < listOfConfigurationItems.Count; reg++)
            {
                InputsRegisters[reg] = listOfConfigurationItems[listIndex++] as ModbusDataPoint<UInt16> ?? new ModbusDataPoint<UInt16>();
            }
            return true;
        }
        public UInt16 Size
        {
            get
            {
                return (UInt16)(((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(ModuleStatus) +
                       InputsRegisters.Aggregate(0,
                           (total, reg) => (UInt16)(total + SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(reg)))) + 1) / 2);
            }
        }
    }
}
