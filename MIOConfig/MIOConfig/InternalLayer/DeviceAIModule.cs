using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceAIModule : IDeviceModule
    {
        public DeviceAIModule()
        {
            ModuleStatus = new DeviceAIModuleStatus();
            Current = new ModbusDataPoint<ushort>();
            Frequency = new ModbusDataPoint<ushort>();
            CurrentDoublePrecision = new ModbusDataPoint<double>();
        }

        public DeviceAIModuleStatus ModuleStatus { get; set; }

        public double CurrentMeasurement
        {
            get { return Current / 1000.0; }
            set { }
        }
        public double FrequencyMeasurement 
        {
            get { return Frequency / 1000.0; }
            set { }
        }

        [field: NonSerialized]
        public ModbusDataPoint<UInt16> Current;

        [field: NonSerialized]
        public ModbusDataPoint<UInt16> Frequency;

        [field: NonSerialized]
        public ModbusDataPoint<double> CurrentDoublePrecision;

        public List<object> ToList()
        {
            List<object> listOfConfigurationItems = new List<object>();
            listOfConfigurationItems.Add(ModuleStatus);
            listOfConfigurationItems.Add(Current);
            listOfConfigurationItems.Add(Frequency);
            listOfConfigurationItems.Add(CurrentDoublePrecision);
            return listOfConfigurationItems;
        }

        public bool FromList(List<object> listOfConfigurationItems)
        {
            if (listOfConfigurationItems.Count < 4)
                return false;
            int listIndex = 0;
            //Header           
            ModuleStatus = listOfConfigurationItems[listIndex++] as DeviceAIModuleStatus ?? new DeviceAIModuleStatus();
            Current = listOfConfigurationItems[listIndex++] as ModbusDataPoint<UInt16> ?? new ModbusDataPoint<UInt16>();
            Frequency = listOfConfigurationItems[listIndex++] as ModbusDataPoint<UInt16> ?? new ModbusDataPoint<UInt16>();
            CurrentDoublePrecision = listOfConfigurationItems[listIndex++] as ModbusDataPoint<double> ?? new ModbusDataPoint<double>();            
            return true;
        }
        public UInt16 Size
        {
            get
            {
                return (UInt16)(SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(ModuleStatus) +
                    SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(Current)+
                    SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(Frequency)+
                    SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(CurrentDoublePrecision));                       
            }
        }
    }
}
