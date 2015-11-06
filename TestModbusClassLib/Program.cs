using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;
using System.IO;
using Tech.CodeGeneration;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using EnumExtension;

#pragma warning disable 1591

namespace TestModbusClassLib
{
    /*class MyClass
    {
        public MyClass()
        {
            a = 0;
            b = 0;
            c = 123.45f;
            d = 0.0;
        }        
        public Int16 a{get;set;}
        public Int32 b{get;set;}
        public Single c{get;set;}
        public Double d{get;set;}

        public void Print()
        {           
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);            
            Console.WriteLine(d);
        }
    }    
    class MyClass2
    {
        public MyClass2()
        {
            a = 0;
            b = 0;          
        }
        public UInt16 a { get; set; }
        public UInt32 b { get; set; }

        public void Print()
        {
            Console.WriteLine(a);
            Console.WriteLine(b);            
        }
    }
    class MyClass3
    {
        public MyClass3()
        {
            a = 0;
            b = 0;
        }
        public UInt16 a { get; set; }
        private UInt32 b { get; set; }

        public void Print()
        {
            Console.WriteLine(a);
            Console.WriteLine(b);
        }
    }*/
    class Program
    {        
        public static void ShowException(Exception exception)
        {
            Console.WriteLine(exception.ToString()+"\t"+exception.StackTrace);
        }
        

        static void Main(string[] args)
        {
            
            Console.ReadLine();
                
            /*Byte[] arr = BitConverterEx.GetBytes(1234567890.123456789m);
            Byte[] arr2 = BitConverterEx.GetBytes(1234567890.123456789m);     
            Array.Reverse(arr);
            StringBuilder stbBuilder= new StringBuilder();
            StringBuilder stbBuilder2= new StringBuilder();
            foreach (var by in arr)
            {
                stbBuilder.Append("0x");
                stbBuilder.AppendFormat("{0}", by.ToString("X"));
                stbBuilder.Append(", ");
                                
            }
            foreach (var by2 in arr2)
            {
                stbBuilder2.Append("0x");
                stbBuilder2.AppendFormat("{0}", by2.ToString("X"));
                stbBuilder2.Append(", ");
            }
            string strtemp = stbBuilder.ToString();
            string strtemp2 = stbBuilder2.ToString();*/
           
            while (true)
            {
                var watch = Stopwatch.StartNew();
                ModbusRtuProtocol prot = new ModbusRtuProtocol();
                watch.Stop();
                Console.WriteLine("new ModbusRTUProtocol(): {0}",watch.ElapsedMilliseconds);

                prot.AddExceptionsLogger(ShowException);

                prot.Connect("COM6");

                ModbusDataPoint<UInt32> dpIn = new ModbusDataPoint<UInt32>();
                dpIn.Value = 123456789;

                ModbusDataPoint<UInt32> dpOut = new ModbusDataPoint<UInt32>();
                dpOut.Value = 0;

                object[] arrayValues = { Byte.MaxValue, UInt16.MinValue, SByte.MinValue, Int16.MinValue, UInt32.MaxValue, Int32.MinValue, Single.MaxValue, UInt64.MaxValue, Int64.MinValue, Double.MaxValue, Decimal.MaxValue };

                object[] arrayValues2 = { (Byte)0, (UInt16)0, (SByte)0, (Int16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };

                watch = Stopwatch.StartNew();  
                ModbusErrorCode code = prot.PresetMultipleRegisters(1, 0, arrayValues);
                watch.Stop();
                Console.WriteLine("PresetMultipleRegisters: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());
                                

                watch = Stopwatch.StartNew();
                List<bool> lstVals = new List<bool> {true, true, true, true, true, true};

                code = prot.ForceMultipleCoils(1, 0, lstVals);
                watch.Stop();
                Console.WriteLine("ForceMultipleCoils: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());

                List<object> lstRegs = new List<object> { Byte.MaxValue, UInt16.MinValue, SByte.MinValue, Int16.MinValue, UInt32.MaxValue, Int32.MinValue, Single.MaxValue, UInt64.MaxValue, Int64.MinValue, Double.MaxValue, Decimal.MaxValue };
                watch = Stopwatch.StartNew();
                code = prot.PresetMultipleRegisters(1, 50, lstRegs);
                watch.Stop();
                Console.WriteLine("PresetMultipleRegisters: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());

                List<object> regs = arrayValues2.ToList();
                watch = Stopwatch.StartNew();
                code = prot.ReadHoldingRegisters(1, 50, ref regs);
                watch.Stop();
                Console.WriteLine("ReadHoldingRegisters: {0}", watch.ElapsedMilliseconds);
                foreach (var reg in regs)
                {
                    Console.WriteLine(reg);
                }

                /*ModbusErrorCode code = prot.PresetSingleRegister(1, 0, (Int16)(-123));
                code = prot.PresetSingleRegister(1, 1, (UInt16)65523);
                code = prot.PresetSingleRegister(1, 2, arrVals);
                */
                /*watch = Stopwatch.StartNew();                
                prot.Connect("COM6");
                watch.Stop();
                Console.WriteLine("Connect: {0}", watch.ElapsedMilliseconds);

                 object[] modbusTestMap = { new ModbusDataPoint<Byte>(), 
                                              new ModbusDataPoint<SByte>(), 
                                              new ModbusDataPoint<Int16>(),                                      
                                              new ModbusDataPoint<UInt16>(), 
                                              new ModbusDataPoint<UInt32>(), 
                                              new ModbusDataPoint<Int32>(), 
                                              new ModbusDataPoint<Single>(), 
                                              new ModbusDataPoint<UInt64>(), 
                                              new ModbusDataPoint<Int64>(),  
                                              new ModbusDataPoint<Double>(), 
                                              new ModbusDataPoint<Decimal>()};
                
                 watch = Stopwatch.StartNew();
                 ModbusErrorCode code = prot.ReadHoldingRegisters(1, 0, ref modbusTestMap);
                 watch.Stop();
                 Console.WriteLine("ReadHoldingRegisters: {0}", watch.ElapsedMilliseconds);
                 
                 Console.WriteLine(((ModbusDataPoint<Byte>)modbusTestMap[0]).Value);
                 Console.WriteLine(((ModbusDataPoint<SByte>)modbusTestMap[1]).Value);
                 Console.WriteLine(((ModbusDataPoint<Int16>)modbusTestMap[2]).Value);
                 Console.WriteLine(((ModbusDataPoint<UInt16>)modbusTestMap[3]).Value);
                 Console.WriteLine(((ModbusDataPoint<UInt32>)modbusTestMap[4]).Value);
                 Console.WriteLine(((ModbusDataPoint<Int32>)modbusTestMap[5]).Value);
                 Console.WriteLine(((ModbusDataPoint<Single>)modbusTestMap[6]).Value);
                 Console.WriteLine(((ModbusDataPoint<UInt64>)modbusTestMap[7]).Value);
                 Console.WriteLine(((ModbusDataPoint<Int64>)modbusTestMap[8]).Value);
                 Console.WriteLine(((ModbusDataPoint<Double>)modbusTestMap[9]).Value);
                 Console.WriteLine(((ModbusDataPoint<Decimal>)modbusTestMap[10]).Value);
                 Console.WriteLine(code.GetDescription());

                 /*Console.ReadLine();

                 watch = Stopwatch.StartNew();
                 code = prot.ReadInputRegisters(1, 0, ref modbusTestMap);
                 watch.Stop();
                 Console.WriteLine("ReadInputRegisters: {0}", watch.ElapsedMilliseconds);

                 Console.WriteLine(((ModbusDataPoint<Byte>)modbusTestMap[0]).Value);
                 Console.WriteLine(((ModbusDataPoint<SByte>)modbusTestMap[1]).Value);
                 Console.WriteLine(((ModbusDataPoint<Int16>)modbusTestMap[2]).Value);
                 Console.WriteLine(((ModbusDataPoint<UInt16>)modbusTestMap[3]).Value);
                 Console.WriteLine(((ModbusDataPoint<UInt32>)modbusTestMap[4]).Value);
                 Console.WriteLine(((ModbusDataPoint<Int32>)modbusTestMap[5]).Value);
                 Console.WriteLine(((ModbusDataPoint<Single>)modbusTestMap[6]).Value);
                 Console.WriteLine(((ModbusDataPoint<UInt64>)modbusTestMap[7]).Value);
                 Console.WriteLine(((ModbusDataPoint<Int64>)modbusTestMap[8]).Value);
                 Console.WriteLine(((ModbusDataPoint<Double>)modbusTestMap[9]).Value);
                 Console.WriteLine(((ModbusDataPoint<Decimal>)modbusTestMap[10]).Value);
                 Console.WriteLine(code.GetDescription());
                 

                bool[] modbusTestMap = new bool[14];
                watch = Stopwatch.StartNew();
                ModbusErrorCode code = prot.ReadCoilStatus(1, 0, ref modbusTestMap);
                watch.Stop();
                Console.WriteLine("ReadCoilStatus: {0}", watch.ElapsedMilliseconds);
                foreach (var item in modbusTestMap)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine(code.GetDescription());
                Console.WriteLine(modbusTestMap.Length);

                watch = Stopwatch.StartNew();
                bool[] arrVals = new[] {true, true, true, true};
                ModbusErrorCode code = prot.ForceMultipleCoils(1, 4, arrVals);
                watch.Stop();
                Console.WriteLine("ForceMultipleCoils: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());

                UInt16[] arrVals16 = new UInt16[4] { 1, 2, 3, 65000 };
                watch = Stopwatch.StartNew();
                code = prot.PresetMultipleRegisters(1, 20, arrVals16);
                watch.Stop();
                Console.WriteLine("PresetMultipleRegisters: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());

                Int16[] arrValsi16 = new Int16[3] { -1, -2, -3 };
                watch = Stopwatch.StartNew();
                code = prot.PresetMultipleRegisters(1, 30, arrValsi16);
                watch.Stop();
                Console.WriteLine("PresetMultipleRegisters: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());
                */
                prot.Disconnect();
                Console.ReadLine();
            }                       
        }      
        
    }
}
