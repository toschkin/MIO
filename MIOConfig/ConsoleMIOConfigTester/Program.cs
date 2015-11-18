﻿using System;
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
        static string GetVariableName<T>(Expression<Func<T>> expr)
        {
            var body = (MemberExpression)expr.Body;

            return body.Member.Name;
        }
        private static void ShowObjectPropsAndVals(object obj)
        {    
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                Console.WriteLine("{0}\t{1}\t{2:X}",GetVariableName(() => obj), prop.Name, prop.GetValue(obj));
            }
        }

        static void Main(string[] args)
        {
            ModbusRtuProtocol protocol = new ModbusRtuProtocol();           
            protocol.AddExceptionsLogger(ShowException);

            if (protocol.Connect("COM6"))
            {
                protocol.ReadRegistersPerQueryCapacity = 5;
                protocol.WriteRegistersPerQueryCapacity = 2;
                ModbusDeviceReaderSaver modbusReader = new ModbusDeviceReaderSaver(protocol,1,1000);
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
                //dpDW2.Value = 0x15141312;*/
                
                //Console.WriteLine(modbusSaver.SaveDeviceConfiguration(listForTest));

                modbusReader.BigEndianOrder = false;

                for (byte i = 1; i <= 14; i++)
                {
                    List<object> listForTest = new List<object> { dpB1, dpW1, dpW2, dpB2, dpW3, dpW4, dpW5, dpW6, dpB3, dpB4, dpDW1, dpB5, dpDW2 };
                    protocol.ReadRegistersPerQueryCapacity = i;                    
                    Console.WriteLine(modbusReader.ReadDeviceConfiguration(ref listForTest));
                    foreach (var item in listForTest)
                    {
                        ShowObjectPropsAndVals(item);
                    }                                      
                }

               

                Console.ReadLine();                

                protocol.Disconnect();
            }            
        }
    }
}