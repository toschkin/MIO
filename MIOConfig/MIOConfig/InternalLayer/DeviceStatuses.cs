using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceStatuses : IDeviceModule
    {
        public DeviceStatuses()
        {
            UartPortStatuses = new List<DeviceUartPortStatus> {new DeviceUartPortStatus()};
            Header = new DeviceStatusesHeader();
        }       
        public DeviceStatusesHeader Header { get; set; }

        public List<DeviceUartPortStatus> UartPortStatuses;

        public List<object> ToList()
        {
            List<object> listOfConfigurationItems = new List<object>();
            listOfConfigurationItems.Add(Header);
            listOfConfigurationItems.AddRange(UartPortStatuses);            
            return listOfConfigurationItems;
        }

        public bool FromList(List<object> listOfConfigurationItems)
        {
            if (listOfConfigurationItems.Count < 2)
                return false;
            int listIndex = 0;
            //Header           
            Header = listOfConfigurationItems[listIndex++] as DeviceStatusesHeader ?? new DeviceStatusesHeader();            
            //UART ports
            for (int port = 0; port < UartPortStatuses.Count && listIndex < listOfConfigurationItems.Count; port++)
            {
                UartPortStatuses[port] = listOfConfigurationItems[listIndex++] as DeviceUartPortStatus ?? new DeviceUartPortStatus();
            }            
            return true;
        }

        public UInt16 Size {
            get
            {
                return (UInt16)(((SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(Header) +
                       UartPortStatuses.Aggregate(0,
                           (total, port) => (UInt16)(total + SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(port))))+1)/2);
            }
        }
    }

}
