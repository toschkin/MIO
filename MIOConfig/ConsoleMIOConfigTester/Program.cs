using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MIOConfig;
using Modbus.Core;

namespace ConsoleMIOConfigTester
{

    class Program
    {
        public static void ShowException(Exception exception)
        {
            Console.WriteLine(exception.ToString() + "\t" + exception.StackTrace);
        }

        private static void ShowObjectPropsAndVals(object obj)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                Console.WriteLine("{0}\t{1}", prop.Name, prop.GetValue(obj));
            }
        }

        static void Main(string[] args)
        {
            ModbusRtuProtocol protocol = new ModbusRtuProtocol();           
            protocol.AddExceptionsLogger(ShowException);

            if (protocol.Connect("COM6"))
            {
                ModbusDeviceReaderSaver modbusReader = new ModbusDeviceReaderSaver(protocol,1,1000,false,4);
                ModbusDeviceReaderSaver modbusSaver = new ModbusDeviceReaderSaver(protocol, 1, 1005);

                /*DeviceConfiguration config = new DeviceConfiguration();

                ShowObjectPropsAndVals(config.DeviceUartPorts[0]);
                Console.WriteLine();    
                ShowObjectPropsAndVals(config.DeviceHeaderFields);
                Console.WriteLine();    

                Console.WriteLine("SaveConfiguration...");
                Console.WriteLine(config.SaveConfiguration(modbusSaver));
                Console.WriteLine();    

                Console.WriteLine("Changing config");
                config.DeviceUartPorts[0].PortOperation = 0;
                config.DeviceUartPorts[0].PortNumber = 1;
                config.DeviceUartPorts[0].PortSpeed = 1200;
                config.DeviceUartPorts[0].PortStopBits = 1;
                config.DeviceUartPorts[0].PortParity = 2;
                config.DeviceUartPorts[0].PortByteSize = 9;
                config.DeviceUartPorts[0].PortFlowControl = 0;
                config.DeviceUartPorts[0].PortProtocolType = 0;
                config.DeviceUartPorts[0].PortMasterTimeout = 300;
                config.DeviceUartPorts[0].PortMasterRequestCount = 1;
                config.DeviceUartPorts[0].PortRetriesCount = 6;
                config.DeviceUartPorts[0].PortModbusAddress = 2;                               
                ShowObjectPropsAndVals(config.DeviceUartPorts[0]);
                Console.WriteLine();                

                
                Console.WriteLine("ReadConfiguration...");
                Console.WriteLine(config.ReadConfiguration(modbusReader));
                Console.WriteLine();   

                
                ShowObjectPropsAndVals(config.DeviceUartPorts[0]);
                Console.WriteLine();    
                ShowObjectPropsAndVals(config.DeviceHeaderFields);
                Console.WriteLine("config.DeviceUartPorts.Count = {0}", config.DeviceUartPorts.Count);    */
                ModbusDataPoint<Byte> dpB1= new ModbusDataPoint<byte>();
                ModbusDataPoint<UInt16> dpW1 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<UInt16> dpW2 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<Byte> dpB2 = new ModbusDataPoint<byte>();

                dpB1.Value = 0x01;
                dpW1.Value = 0xFFFF;
                dpW2.Value = 0xAAAA;
                dpB2.Value = 0x02;

                List<object> listForTest = new List<object> {dpB1,dpW1,dpW2,dpB2};
                modbusReader.ReadDeviceConfiguration(ref listForTest);
                protocol.Disconnect();
            }

            Console.ReadLine();
        }
    }
}
