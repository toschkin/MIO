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
    class MyClass
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
    }
    class Program
    {
        static Type[] GetObjectPropertiesTypesArray(object obj)
        {
            List<Type> lstTypesOfClassMembers = new List<Type>();
            PropertyInfo[] membersOfClass = obj.GetType().GetProperties();
            foreach (var item in membersOfClass)
            {
                if (item.PropertyType.IsPublic)
                    lstTypesOfClassMembers.Add(item.PropertyType);
            }
            return lstTypesOfClassMembers.ToArray<Type>();
        }

        static bool SetObjectPropertiesFromArray(ref object obj, ValueType[] arrayValues)
        {
            if (obj.GetType().GetProperties().Length == arrayValues.Length)
            {
                //first we check that arrayValues has same value types with object properties
                int i = 0;
                foreach (var item in obj.GetType().GetProperties())
                {
                    if (item.PropertyType == arrayValues[i].GetType())
                        i++;
                    else
                        return false;
                }
                //setting values from arrayValues to corresponding properties  
                i = 0;
                foreach (var item in obj.GetType().GetProperties())
                {
                    item.SetValue(obj, Convert.ChangeType(arrayValues[i], item.PropertyType));
                    i++;
                }
                return true;
            }
            else
                return false;
        }
       
        static ValueType[] CreateArrayByTypes(Type[] arrTypes)
        {
            List<ValueType> listOfElem = new List<ValueType>();
            for (int i = 0; i < arrTypes.Length; i++)
            {
                if(arrTypes[i].Name == "Int16")
                {
                    Int16 a = -321;
                    listOfElem.Add(a);
                }                
                if (arrTypes[i].Name == "UInt16")
                {
                    UInt16 a = 54321;
                    listOfElem.Add(a);
                }
                if (arrTypes[i].Name == "Int32")
                {
                    Int32 a = -654321;
                    listOfElem.Add(a);
                }
                if (arrTypes[i].Name == "UInt32")
                {
                    UInt32 a = 654321;
                    listOfElem.Add(a);
                }
                if (arrTypes[i].Name == "Single")
                {
                    Single a = -123.456f;
                    listOfElem.Add(a);
                }
                if (arrTypes[i].Name == "Double")
                {
                    Double a = -654.321;
                    listOfElem.Add(a);
                }
            }
            return listOfElem.ToArray < ValueType >();
        }
        static void Main(string[] args)
        {                                             
            Byte[] arr = BitConverterEx.GetBytes(1234567890.123456789m);
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
            string strtemp2 = stbBuilder2.ToString();

           

            while (true)
            {
                var watch = Stopwatch.StartNew();
                ModbusRTUProtocol prot = new ModbusRTUProtocol();
                watch.Stop();
                Console.WriteLine("new ModbusRTUProtocol(): {0}",watch.ElapsedMilliseconds);

                watch = Stopwatch.StartNew();                
                prot.Connect("COM6");
                watch.Stop();
                Console.WriteLine("Connect: {0}", watch.ElapsedMilliseconds);

                /* object[] modbusTestMap = { new ModbusDataPoint<Byte>(), 
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
                Console.WriteLine(modbusTestMap.Length);*/

                watch = Stopwatch.StartNew();
                ModbusErrorCode code = prot.ForceSingleCoil(1, 4, true);
                watch.Stop();
                Console.WriteLine("ForceSingleCoil: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());

                watch = Stopwatch.StartNew();
                code = prot.PresetSingleRegister(1, 14, 37000);
                watch.Stop();
                Console.WriteLine("PresetSingleRegister: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());

                watch = Stopwatch.StartNew();
                code = prot.PresetSingleRegister(1, 15, -1);
                watch.Stop();
                Console.WriteLine("PresetSingleRegister: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine(code.GetDescription());

                prot.Disconnect();
                Console.ReadLine();
            }                       
        }      
        
    }
}
