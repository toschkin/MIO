using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceConfigurationTime
    {
        public DateTime ConfigurationTime { get; set; }

        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt32 RegisterValue
        {
            get
            {
                UInt32 registerValue = 0;
                registerValue |= (UInt32) (ConfigurationTime.Second << 26);
                registerValue |= (UInt32) (ConfigurationTime.Minute << 20);
                registerValue |= (UInt32) (ConfigurationTime.Hour << 15);
                registerValue |= (UInt32) ((ConfigurationTime.Year % 1000) << 9);
                registerValue |= (UInt32) (ConfigurationTime.Month << 5);
                registerValue |= (UInt32) (ConfigurationTime.Day);
                return registerValue;
            }
            set
            {               
                ConfigurationTime = new DateTime((int)((value & 0x00007E00) >> 9) + 2000,
                                                        (int)(value & 0x000001E0) >> 5,
                                                        (int)(value & 0x0000001F),
                                                        (int)(value & 0x000F8000) >> 15,
                                                        (int)(value & 0x03F00000)>>20,
                                                        (int)((value & 0xFC000000) >> 26));                
            }
        }

        public override string ToString()
        {
            StringBuilder resultString = new StringBuilder();
            resultString.AppendFormat("Время последней конфигурации: {0}\r\n", ConfigurationTime);
            return resultString.ToString();
        }
    }
}
