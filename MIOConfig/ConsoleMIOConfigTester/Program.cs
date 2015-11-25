using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MIOConfig;
using Modbus.Core;
using EnumExtension;

namespace ConsoleMIOConfigTester
{

    class Program
    {
        public static void ShowException(Exception exception)
        {
            Console.WriteLine(exception.ToString() + "\t" + exception.StackTrace);
        }

        public static void ShowError(string error)
        {
            Console.WriteLine("DeviceConfiguration: {0}",error);
        }

        static string GetVariableName<T>(Expression<Func<T>> expr)
        {
            var body = (MemberExpression)expr.Body;

            return body.Member.Name;
        }
        private static void ShowObjectPropsAndVals(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("NULL");
                return;
            }                
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                Console.WriteLine("{0}\t{1}\t{2}",GetVariableName(() => obj), prop.Name, prop.GetValue(obj));
            }
        }

        static void Main(string[] args)
        {
            MIOConfig.
            Device device = new Device();
            device.AddErrorLogger(ShowError);

            ModbusRtuProtocol protocol = new ModbusRtuProtocol();           
            protocol.AddExceptionsLogger(ShowException);
                      

            FileReaderSaver fileReaderSaver = new FileReaderSaver(@"c:\device.dat");
            //FileReaderSaver fileSaver = new FileReaderSaver(@"c:\device.dat");                    

            if (protocol.Connect("COM6"))
            {
                protocol.ReadRegistersPerQueryCapacity = 125;
                protocol.WriteRegistersPerQueryCapacity = 125;
                ModbusReaderSaver modbusReaderSaver = new ModbusReaderSaver(protocol, 1, 1000, 1005);

                
                Console.WriteLine("ReadConfiguration...");

                Console.WriteLine(device.ReadConfiguration(modbusReaderSaver).GetDescription());                
                //Console.WriteLine(modbusReaderSaver.ReadDeviceConfiguration(ref device).GetDescription());
                Console.WriteLine(device);
                foreach (var item in device.UartPortsConfigurations)
                {
                    ShowObjectPropsAndVals(item);
                }
                Console.WriteLine();
                Console.ReadLine();

                device.DiscreetInputModule.ModuleOperation = 0;
                device.DiscreetInputModule.HysteresisTime = 0xAA55;

                device.DiscreetOutputModule.ModuleOperation = 0;
                device.DiscreetOutputModule.PulseDurationTime = 0xBB66;

                device.RoutingMap[1].RouteFrom = 0x300;
                device.RoutingMap[1].RouteTo = 0x400;

                Console.WriteLine("SaveConfiguration...Modbus");
                Console.WriteLine(device.SaveConfiguration(modbusReaderSaver).GetDescription());
                Console.ReadLine();

                Console.WriteLine("SaveConfiguration...File");
                Console.WriteLine(device.SaveConfiguration(fileReaderSaver).GetDescription());
                Console.ReadLine();

                Console.WriteLine("clearing...");
                device = new Device();
                Console.ReadLine();

                Console.WriteLine("ReadConfiguration...File");
                Console.WriteLine(device.ReadConfiguration(fileReaderSaver).GetDescription());
                Console.WriteLine(device);
                foreach (var item in device.UartPortsConfigurations)
                {
                    ShowObjectPropsAndVals(item);
                }
                Console.WriteLine();

                protocol.Disconnect();            
            }
            Console.ReadLine();
            /*Console.WriteLine("Start...");                
                Console.WriteLine(config);
               

                Console.WriteLine("SaveConfiguration...");
                Console.WriteLine(config.SaveConfiguration(modbusSaver));     
                Console.WriteLine(config);
                Console.WriteLine();

                Console.WriteLine("clear Configuration...");
                config = new DeviceConfiguration();
                Console.WriteLine(config);
                Console.WriteLine();

                Console.WriteLine("ReadConfiguration...");
                Console.WriteLine(config.ReadConfiguration(modbusReader));                
                Console.WriteLine(config);
                foreach (var item in config.UartPorts)
                {
                    ShowObjectPropsAndVals(item);
                }
                Console.WriteLine();
                ShowObjectPropsAndVals(config.DIModule);
                

                Console.WriteLine("SaveConfiguration to {0}", fileSaver.FilePath);
                Console.WriteLine(config.SaveConfiguration(fileSaver));
                
                Console.WriteLine("clear Configuration...");
                config = new DeviceConfiguration();
                Console.WriteLine(config);
                Console.ReadLine();

                Console.WriteLine("ReadConfiguration from {0}", fileReader.FilePath);
                Console.WriteLine(config.ReadConfiguration(fileReader));
                Console.WriteLine(config);
                foreach (var item in config.UartPorts)
                {
                    ShowObjectPropsAndVals(item);
                }
                Console.WriteLine();
                ShowObjectPropsAndVals(config.DIModule);
                Console.ReadLine();

               

                /*Console.WriteLine("Changing config");
                config.UartPorts[0].PortOperation = 0;
                config.DeviceUartPorts[0].PortNumber = 1;
                config.UartPorts[0].PortSpeed = 1200;
                config.UartPorts[0].PortStopBits = 1;
                config.UartPorts[0].PortParity = 2;
                config.UartPorts[0].PortByteSize = 9;
                config.UartPorts[0].PortFlowControl = 0;
                config.UartPorts[0].PortProtocolType = 0;
                config.UartPorts[0].PortMasterTimeout = 300;
                config.UartPorts[0].PortMasterRequestCount = 1;
                config.UartPorts[0].PortRetriesCount = 6;
                config.UartPorts[0].PortModbusAddress = 2;                               
                ShowObjectPropsAndVals(config.UartPorts[0]);
                Console.WriteLine();                

                
                Console.WriteLine("ReadConfiguration...");
                Console.WriteLine(config.ReadConfiguration(modbusReader));
                Console.WriteLine();


                ShowObjectPropsAndVals(config.HeaderFields);
                Console.WriteLine();    
                ShowObjectPropsAndVals(config.UartPorts[0]);
                                
                Console.WriteLine("config.UartPorts.Count = {0}", config.UartPorts.Count);    

                /*ModbusDataPoint<Byte> dpB1= new ModbusDataPoint<byte>();
                ModbusDataPoint<UInt16> dpW1 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<UInt16> dpW2 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<Byte> dpB2 = new ModbusDataPoint<byte>();
                ModbusDataPoint<UInt16> dpW3 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<UInt16> dpW4 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<UInt16> dpW5 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<UInt16> dpW6 = new ModbusDataPoint<UInt16>();
                ModbusDataPoint<Byte> dpB3 = new ModbusDataPoint<byte>();
                ModbusDataPoint<Byte> dpB4 = new ModbusDataPoint<byte>();
                ModbusDataPoint<UInt32> dpDW1 = new ModbusDataPoint<UInt32>();
                ModbusDataPoint<Byte> dpB5 = new ModbusDataPoint<byte>();
                ModbusDataPoint<Int32> dpDW2 = new ModbusDataPoint<Int32>();
                
                
                /*dpB1.Value = 0xAA;
                dpW1.Value = 0xBBAA;
                dpW2.Value = 0xCCBB;
                dpB2.Value = 0xCC;
                dpW3.Value = 0xDDDD;
                dpW4.Value = 0xEEEE;
                dpW5.Value = 0xFFFF;
                dpW6.Value = 0x0403;
                dpB3.Value = 0x05;
                dpB4.Value = 0x06;
                //dpDW1.Value = 0x10090807;
                //dpB5.Value = 0x11;
                //dpDW2.Value = 0x15141312;
                
                //Console.WriteLine(modbusSaver.SaveDeviceConfiguration(listForTest));

                modbusReader.BigEndianOrder = false;
                bool ret = false;
                for (byte i = 1; i <= 14; i++)
                {
                    List<object> listForTest = new List<object> { dpB1, dpW1, dpW2, dpB2, dpW3, dpW4, dpW5, dpW6, dpB3, dpB4, dpDW1, dpB5, dpDW2 };
                    protocol.ReadRegistersPerQueryCapacity = i;
                    protocol.WriteRegistersPerQueryCapacity = i;                    
                    var watch = Stopwatch.StartNew();
                    ret = modbusReader.ReadDeviceConfiguration(ref listForTest);
                    watch.Stop();
                    Console.WriteLine("Read = {0}",ret);
                    foreach (var item in listForTest)
                    {
                        ShowObjectPropsAndVals(item);
                    }                   
                    
                    protocol.ReadRegistersPerQueryCapacity = i;
                    watch = Stopwatch.StartNew();
                    ret = modbusSaver.SaveDeviceConfiguration(listForTest);
                    watch.Stop();
                    Console.WriteLine("Save = {0}", ret);
                    Console.WriteLine(watch.ElapsedMilliseconds);
                    Console.ReadLine();              
                }
                



                 */
        }
    }
}
