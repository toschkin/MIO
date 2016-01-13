using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MIOConfigurator
{
    class ModbusCommunicationParameters
    {
        private string _registryNode;
       
        public ModbusCommunicationParameters(string registryNode)
        {
            _registryNode = registryNode;
        }

        static public ObservableCollection<string> AvailableComPorts
        {
            get
            {
                return new ObservableCollection<string>(SerialPort.GetPortNames());                
            }
        }

        static public ObservableCollection<int> AvailableComSpeeds
        {
            get
            {                
                ObservableCollection<int> speeds = new ObservableCollection<int>
                {
                    75,
	                100,
	                110,
	                134,
	                150,
	                200,
	                300,
	                600,
	                1200,
	                1800,
	                2400,
	                4800,
	                7200,
	                9600,
	                14400,
	                19200,
	                38400,
	                56000,	
	                57600,
	                64000,
	                115200,
	                128000
                };
                return speeds;
            }
        }

        static public ObservableCollection<Byte> AvailableComByteSizes
        {
            get
            {
                ObservableCollection<Byte> sizes = new ObservableCollection<Byte>
                {
                    7,
                    8,
                    9                    
                };
                return sizes;
            }
        }

        static public ObservableCollection<Parity> AvailableComParities
        {
            get
            {                
                ObservableCollection<Parity> parities = new ObservableCollection<Parity>
                {                    
                    Parity.None,
                    Parity.Even,
                    Parity.Odd,
                    Parity.Mark,
                    Parity.Space
                };
                return parities;
            }
        }

        static public ObservableCollection<StopBits> AvailableComStopBits
        {
            get
            {
                ObservableCollection<StopBits> stopBits = new ObservableCollection<StopBits>
                {
                    StopBits.None,
                    StopBits.One,
                    StopBits.Two,
                    StopBits.OnePointFive
                };
                return stopBits;
            }
        }

        public string DefaultComPort 
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return (string)Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("Port", "COM1");
                return "COM1";
            }
            set
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("Port", value);
            }
        }

        public int DefaultComSpeed 
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return Convert.ToInt32(Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("Speed", "9600"));
                return 9600;
            }
            set
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("Speed", value.ToString());    
            }
        }

        public int DefaultComByteSize
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return Convert.ToInt32(Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("ByteSize", "8"));
                return 8;
            }
            set
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("ByteSize", value.ToString());
            }
        }

        public Parity DefaultComParity
        {
            get
            {                
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return (Parity)Enum.Parse(typeof(Parity),(string)Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("Parity", Parity.None.ToString()));
                return Parity.None;
            }
            set
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("Parity", value.ToString());
            }
        }

        public StopBits DefaultComStopBits
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return (StopBits)Enum.Parse(typeof(StopBits), (string)Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("StopBits", StopBits.Two.ToString()));
                return StopBits.One;
            }
            set
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("StopBits", value.ToString());
            }
        }

        public int DefaultModbusTimeOut
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return Convert.ToInt32(Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("TimeOut", "1000"));
                return 1000;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("TimeOut", value.ToString());
            }
        }

        public int DefaultModbusSilentInterval
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return Convert.ToInt32(Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("SilentInterval", "20"));
                return 20;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("SilentInterval", value.ToString());
            }
        }
        public int DefaultModbusWriteCapacity
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return Convert.ToInt32(Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("WriteCapacity", "123"));
                return 123;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("WriteCapacity", value.ToString());
            }
        }
        public int DefaultModbusReadCapacity
        {
            get
            {
                if (Registry.CurrentUser.OpenSubKey(_registryNode) != null)
                    return Convert.ToInt32(Registry.CurrentUser.OpenSubKey(_registryNode).GetValue("ReadCapacity", "125"));
                return 125;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (Registry.CurrentUser.OpenSubKey(_registryNode, RegistryKeyPermissionCheck.ReadWriteSubTree) != null)
                    Registry.CurrentUser.OpenSubKey(Constants.registryAppNode, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("ReadCapacity", value.ToString());
            }
        }
    }
}
