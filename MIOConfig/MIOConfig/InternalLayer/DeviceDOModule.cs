using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{    
    public class DeviceDOModule : IDeviceModule
    {
        public DeviceDOModule()
        {
            ModuleStatus = new DeviceDOModuleStatus();
            for (int idx = 0; idx < 4; idx++)
            {
                ChannelStatuses[idx] = new DeviceDOModuleStatus();
                RealCoilStates[idx] = new ModbusDataPoint<UInt16>();
                DesiredCoilStates[idx] = new ModbusDataPoint<UInt16>();
            }             
        }

        public DeviceDOModuleStatus ModuleStatus { get; set; }
        [field: NonSerialized]
        public ModbusDataPoint<UInt16>[] RealCoilStates = new ModbusDataPoint<UInt16>[4];
        [field: NonSerialized]
        public ModbusDataPoint<UInt16>[] DesiredCoilStates = new ModbusDataPoint<UInt16>[4];
        public DeviceDOModuleStatus[] ChannelStatuses = new DeviceDOModuleStatus[4];

        
        public string RealCoilState1 { get { return GetCoilStateDescription(RealCoilStates[0]); } }        
        public string RealCoilState2 { get { return GetCoilStateDescription(RealCoilStates[1]); } }        
        public string RealCoilState3 { get { return GetCoilStateDescription(RealCoilStates[2]); } }        
        public string RealCoilState4 { get { return GetCoilStateDescription(RealCoilStates[3]); } }
       
        public string DesiredCoilState1 { get { return GetCoilStateDescription(DesiredCoilStates[0]); } }       
        public string DesiredCoilState2 { get { return GetCoilStateDescription(DesiredCoilStates[1]); } }        
        public string DesiredCoilState3 { get { return GetCoilStateDescription(DesiredCoilStates[2]); } }        
        public string DesiredCoilState4 { get { return GetCoilStateDescription(DesiredCoilStates[3]); } }

        public UInt16 DesiredCoilStateNumeric1 { get { return DesiredCoilStates[0]; } }
        public UInt16 DesiredCoilStateNumeric2 { get { return DesiredCoilStates[1]; } }
        public UInt16 DesiredCoilStateNumeric3 { get { return DesiredCoilStates[2]; } }
        public UInt16 DesiredCoilStateNumeric4 { get { return DesiredCoilStates[3]; } }

        public List<object> ToList()
        {
            List<object> listOfConfigurationItems = new List<object>();
            listOfConfigurationItems.Add(ModuleStatus);
            listOfConfigurationItems.AddRange(RealCoilStates);
            listOfConfigurationItems.AddRange(DesiredCoilStates);
            listOfConfigurationItems.AddRange(ChannelStatuses);
            return listOfConfigurationItems;
        }

        private string GetCoilStateDescription(ModbusDataPoint<UInt16> coilDataPoint)
        {
            switch (coilDataPoint.Value)
            {
                case Definitions.DO_COIL_STATE_OFF:
                    return "ОТКЛ.";
                case Definitions.DO_COIL_STATE_ON:
                    return "ВКЛ.";
                case Definitions.DO_COIL_STATE_INDEF:
                    return "НЕОПРЕД.";
                default:
                    return "НЕОПРЕД.";
            }            
        }
        public bool FromList(List<object> listOfConfigurationItems)
        {
            if (listOfConfigurationItems.Count < Size)
                return false;
            int listIndex = 0;
            
            ModuleStatus = listOfConfigurationItems[listIndex++] as DeviceDOModuleStatus;
            
            for (int reg = 0; reg < RealCoilStates.Length && listIndex < listOfConfigurationItems.Count; reg++)
            {
                RealCoilStates[reg] = listOfConfigurationItems[listIndex++] as ModbusDataPoint<UInt16> ?? new ModbusDataPoint<UInt16>();
            }

            for (int reg = 0; reg < DesiredCoilStates.Length && listIndex < listOfConfigurationItems.Count; reg++)
            {
                DesiredCoilStates[reg] = listOfConfigurationItems[listIndex++] as ModbusDataPoint<UInt16> ?? new ModbusDataPoint<UInt16>();
            }

            for (int reg = 0; reg < ChannelStatuses.Length && listIndex < listOfConfigurationItems.Count; reg++)
            {
                ChannelStatuses[reg] = listOfConfigurationItems[listIndex++] as DeviceDOModuleStatus ?? new DeviceDOModuleStatus();
            }
            return true;
        }
        public UInt16 Size
        {
            get
            {
                return (UInt16)(((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(ModuleStatus) +
                       RealCoilStates.Aggregate(0,(total, reg) => (UInt16)(total + SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(reg))) +
                       DesiredCoilStates.Aggregate(0,(total, reg) => (UInt16)(total + SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(reg))) +
                       ChannelStatuses.Aggregate(0,(total, reg) => (UInt16)(total + SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(reg))))+ 1) / 2);
            }
        }
    }
}
