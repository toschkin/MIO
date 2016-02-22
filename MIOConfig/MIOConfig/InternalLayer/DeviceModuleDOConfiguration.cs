using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceModuleDOConfiguration : INotifyPropertyChanged
    {
        private DeviceConfiguration _deviceConfiguration;

        public DeviceModuleDOConfiguration()
        {
            ModuleOperation = 1;
            PulseDurationTime = 10;
            _deviceConfiguration = null;
        }

        public DeviceModuleDOConfiguration(DeviceConfiguration deviceConfiguration)
        {
            _deviceConfiguration = deviceConfiguration;
        }

        private Byte _moduleOperation;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - out of operation, 1 - in operation</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ModuleOperation
        {
            get { return _moduleOperation; }
            set { _moduleOperation = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent HiByte + 1007+7*UartPorts.Count+5*ModuleDIPresent+1 LoByte |count: 2| R/W
        /// </summary>
        /// <value>range: 0..65535</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 PulseDurationTime { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+1 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - out of operation, 1 - in operation</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ResevedByte1 { get; set; }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+2 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        /// 
        private Byte _outputType1;
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType1
        {
            get { return _outputType1; }
            set
            {
                if ((_outputType1 == (Byte)OutputTypes.DoubleOutput) && (value != (Byte)OutputTypes.DoubleOutput))
                {
                    OutputType2 = 0;
                    ParallelChannelForOutput2 = 0;
                }
                if (value == (Byte)OutputTypes.DoubleOutput)
                {
                    OutputType2 = value;                                        
                }                    
                _outputType1 = value;
                NotifyPropertyChanged("OutputType1");
            }
        }

        private Byte _parallelChannelPresentForOutput1;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+2 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput1
        {
            get { return _parallelChannelPresentForOutput1; }
            set { _parallelChannelPresentForOutput1 = value > 1 ? (Byte)1 : value; }  
        } 

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+3|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput1 { get; set; }

        /// <summary>
        /// For interface only
        /// </summary>
        /// <value>0 - no channel, address otherwise</value>        
        public UInt16 ParallelChannelForOutput1
        {
            get
            {
                if (ParallelChannelPresentForOutput1==0)
                    return 0;

                UInt16 diModuleSize = 0;
                if (_deviceConfiguration != null)
                    diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                        ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                        : 0);

                return (UInt16)(AddressOfParallelChannelForOutput1 - 
                    (Definitions.DEVICE_STATE_OFFSET + 
                     Definitions.DEVICE_STATE_HEADER_SIZE +
                     (_deviceConfiguration?.UartPorts.Count ?? 0) + 
                     diModuleSize + 5) + 1);
            }
            set
            {
                if (value == 0)
                    ParallelChannelPresentForOutput1 = 0;
                else
                {
                    ParallelChannelPresentForOutput1 = 1;

                    UInt16 diModuleSize = 0;
                    if (_deviceConfiguration != null)
                        diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                            ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                            : 0);

                    AddressOfParallelChannelForOutput1 = (UInt16)((value-1) + (Definitions.DEVICE_STATE_OFFSET + 
                        Definitions.DEVICE_STATE_HEADER_SIZE +
                        (_deviceConfiguration?.UartPorts.Count ?? 0) +
                        diModuleSize + 5));                    
                }
                NotifyPropertyChanged("ParallelChannelForOutput1");
            }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+4 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        private Byte _outputType2;
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType2
        {
            get { return _outputType2; }
            set
            {
                _outputType2 = value;
                NotifyPropertyChanged("OutputType2");
            }
        }

        private Byte _parallelChannelPresentForOutput2;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+4 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput2
        {
            get { return _parallelChannelPresentForOutput2; }
            set { _parallelChannelPresentForOutput2 = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+5|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput2 { get; set; }

        /// <summary>
        /// For interface only
        /// </summary>
        /// <value>0 - no channel, address otherwise</value>        
        public UInt16 ParallelChannelForOutput2
        {
            get
            {
                if (ParallelChannelPresentForOutput2 == 0)
                    return 0;

                UInt16 diModuleSize = 0;
                if (_deviceConfiguration != null)
                    diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                        ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                        : 0);

                return (UInt16)(AddressOfParallelChannelForOutput2 - 
                    (Definitions.DEVICE_STATE_OFFSET + 
                    Definitions.DEVICE_STATE_HEADER_SIZE +
                    (_deviceConfiguration?.UartPorts.Count ?? 0) +
                    diModuleSize + 5) + 1);
            }
            set
            {
                switch (ParallelChannelForOutput2)
                {
                    case 3:
                        {
                            if (OutputType3 == (Byte)OutputTypes.ParallelChannel)
                            {
                                OutputType3 = (Byte)OutputTypes.NoOutput;                                
                                ParallelChannelForOutput3 = 0;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (OutputType4 == (Byte)OutputTypes.ParallelChannel)
                            {
                                OutputType4 = (Byte)OutputTypes.NoOutput;                                
                                ParallelChannelForOutput4 = 0;
                            }
                            break;
                        }
                }
                if (value == 0)
                {                    
                    ParallelChannelPresentForOutput2 = 0;
                    AddressOfParallelChannelForOutput2 = 0;
                }                    
                else
                {
                    ParallelChannelPresentForOutput2 = 1;

                    UInt16 diModuleSize = 0;
                    if (_deviceConfiguration != null)
                        diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                            ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                            : 0);

                    AddressOfParallelChannelForOutput2 = (UInt16)((value - 1) + 
                        (Definitions.DEVICE_STATE_OFFSET + 
                        Definitions.DEVICE_STATE_HEADER_SIZE +
                        (_deviceConfiguration?.UartPorts.Count ?? 0) +
                        diModuleSize + 5));
                    if (OutputType2 != (Byte)OutputTypes.ParallelChannel)
                    {
                        if (value == 3) //3rd channel
                        {
                            OutputType3 = (Byte)OutputTypes.ParallelChannel;//parallel                            
                            ParallelChannelPresentForOutput3 = 1;
                            ParallelChannelForOutput3 = 2;
                        }
                        if (value == 4) //3rd channel
                        {
                            OutputType4 = (Byte)OutputTypes.ParallelChannel;//parallel                            
                            ParallelChannelForOutput4 = 2;
                        }   
                    }                    
                }
                NotifyPropertyChanged("ParallelChannelForOutput2");
            }
        }
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+6 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        /// 
        private Byte _outputType3;
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType3
        {
            get { return _outputType3; }
            set
            {
                if ((_outputType3 == (Byte)OutputTypes.DoubleOutput) && (value != (Byte)OutputTypes.DoubleOutput))
                {
                    OutputType4 = 0;                    
                    ParallelChannelForOutput4 = 0;
                }
                if (value == (Byte)OutputTypes.DoubleOutput)
                {
                    OutputType4 = value;                             
                }
                _outputType3 = value;
                NotifyPropertyChanged("OutputType3");
            }
        }

        private Byte _parallelChannelPresentForOutput3;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+6 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput3
        {
            get { return _parallelChannelPresentForOutput3; }
            set { _parallelChannelPresentForOutput3 = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+7|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput3 { get; set; }

        /// <summary>
        /// For interface only
        /// </summary>
        /// <value>0 - no channel, address otherwise</value>        
        public UInt16 ParallelChannelForOutput3
        {
            get
            {
                if (ParallelChannelPresentForOutput3 == 0)
                    return 0;

                UInt16 diModuleSize = 0;
                if (_deviceConfiguration != null)
                    diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                        ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                        : 0);

                return (UInt16)(AddressOfParallelChannelForOutput3 - 
                    (Definitions.DEVICE_STATE_OFFSET+ 
                    Definitions.DEVICE_STATE_HEADER_SIZE +
                    (_deviceConfiguration?.UartPorts.Count ?? 0) +
                    diModuleSize + 5) + 1);
            }
            set
            {
                if (value == 0)
                    ParallelChannelPresentForOutput3 = 0;
                else
                {
                    ParallelChannelPresentForOutput3 = 1;

                    UInt16 diModuleSize = 0;
                    if (_deviceConfiguration != null)
                        diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                            ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                            : 0);

                    AddressOfParallelChannelForOutput3 = (UInt16)((value - 1) + 
                        (Definitions.DEVICE_STATE_OFFSET + 
                        Definitions.DEVICE_STATE_HEADER_SIZE +
                        (_deviceConfiguration?.UartPorts.Count ?? 0) +
                        diModuleSize + 5));
                }
                NotifyPropertyChanged("ParallelChannelForOutput3");
            }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+8 LoByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not in operation, 1 - SP, 2 - DP, 3-parallel</value>
        private Byte _outputType4;
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte OutputType4
        {
            get { return _outputType4; }
            set
            {
                _outputType4 = value;
                NotifyPropertyChanged("OutputType4");
            }
        }

        private Byte _parallelChannelPresentForOutput4;
        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+8 HiByte|count: 1| R/W
        /// </summary>
        /// <value>0 - not present, 1 - present</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ParallelChannelPresentForOutput4
        {
            get { return _parallelChannelPresentForOutput4; }
            set { _parallelChannelPresentForOutput4 = value > 1 ? (Byte)1 : value; }
        }

        /// <summary>
        /// Holding regs|addr.: 1007+7*UartPorts.Count+5*ModuleDIPresent+9|count: 1| R/W
        /// </summary>
        /// <value>0..65535 acording to data map</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 AddressOfParallelChannelForOutput4 { get; set; }

        /// <summary>
        /// For interface only
        /// </summary>
        /// <value>0 - no channel, address otherwise</value>        
        public UInt16 ParallelChannelForOutput4
        {
            get
            {
                if (ParallelChannelPresentForOutput4 == 0)
                    return 0;

                UInt16 diModuleSize = 0;
                if (_deviceConfiguration != null)
                    diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                        ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                        : 0);

                return (UInt16)(AddressOfParallelChannelForOutput4 - (Definitions.DEVICE_STATE_OFFSET + 
                    Definitions.DEVICE_STATE_HEADER_SIZE +
                    (_deviceConfiguration?.UartPorts.Count ?? 0) +
                    diModuleSize + 5) + 1);
            }
            set
            {
                switch (ParallelChannelForOutput4)
                {
                    case 1:
                        {
                            if (OutputType1 == (Byte)OutputTypes.ParallelChannel)
                            {
                                OutputType1 = (Byte)OutputTypes.NoOutput;                                
                                ParallelChannelForOutput1 = 0;
                            }
                            break;
                        }
                    case 2:
                        {
                            if (OutputType2 == (Byte)OutputTypes.ParallelChannel)
                            {
                                OutputType2 = (Byte)OutputTypes.NoOutput;                               
                                ParallelChannelForOutput2 = 0;
                            }
                            break;
                        }
                }
                if (value == 0)
                {                    
                    ParallelChannelPresentForOutput4 = 0;
                    AddressOfParallelChannelForOutput4 = 0;
                }                    
                else
                {
                    ParallelChannelPresentForOutput4 = 1;

                    UInt16 diModuleSize = 0;
                    if (_deviceConfiguration != null)
                        diModuleSize = (UInt16)(_deviceConfiguration.HeaderFields.ModuleDI
                            ? Definitions.DEVICE_DI_MODULE_MAP_SIZE
                            : 0);

                    AddressOfParallelChannelForOutput4 = (UInt16)((value - 1) + 
                        (Definitions.DEVICE_STATE_OFFSET + 
                        Definitions.DEVICE_STATE_HEADER_SIZE +
                        (_deviceConfiguration?.UartPorts.Count ?? 0) +
                        diModuleSize + 5));
                    if (OutputType4 != (Byte)OutputTypes.ParallelChannel)//parallel
                    {
                        if (value == 1) //3rd channel
                        {
                            OutputType1 = (Byte)OutputTypes.ParallelChannel;//parallel                            
                            ParallelChannelForOutput1 = 4;
                        }
                        if (value == 2) //3rd channel
                        {
                            OutputType2 = (Byte)OutputTypes.ParallelChannel;//parallel                            
                            ParallelChannelForOutput2 = 4;
                        }                       
                    }                    
                }
                NotifyPropertyChanged("ParallelChannelForOutput4");
            }
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
