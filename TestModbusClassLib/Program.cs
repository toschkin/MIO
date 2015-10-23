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
            MyClass cl = new MyClass();
            ValueType[] arrOutput = CreateArrayByTypes(GetObjectPropertiesTypesArray(cl));

            Console.WriteLine("Array for MyClass:");
            for (int i = 0; i < arrOutput.Length; i++)
            {
                Console.WriteLine(arrOutput[i]);
            }

            MyClass2 cl2 = new MyClass2();
            ValueType[] arrOutput2 = CreateArrayByTypes(GetObjectPropertiesTypesArray(cl2));

            Console.WriteLine("Array for MyClass2:");
            for (int i = 0; i < arrOutput2.Length; i++)
            {
                Console.WriteLine(arrOutput2[i]);
            }
                      
            object tmp = (object)cl;            
            Console.WriteLine("InitObjectFromArray for MyClass:");
            SetObjectPropertiesFromArray(ref tmp, arrOutput);
            cl.Print();

            tmp = (object)cl2;    
            Console.WriteLine("InitObjectFromArray for MyClass2:");
            SetObjectPropertiesFromArray(ref tmp, arrOutput2);
            cl2.Print();

            object[] rtuData = { new Int16(),  new UInt32(), new Double(), cl, cl2};
            
            int totalLengthInBytesOfRequestedData = 0;
            foreach (var element in rtuData)
            {
                if (element.GetType().IsValueType)//here we will process simple (only numeric) types
                {
                    if (GetTypeHelper.IsNumericType(element.GetType()))
                    {
                        totalLengthInBytesOfRequestedData += Marshal.SizeOf(element);
                        Console.WriteLine("{0}\t{1}", element.GetType().Name, Marshal.SizeOf(element));
                    }
                }
                else//here we will process properties (only numeric) from complex (class) types 
                {
                    foreach (var field in element.GetType().GetProperties())
                    {
                        totalLengthInBytesOfRequestedData += Marshal.SizeOf(field.PropertyType);
                    }                                                                                 
                    //Type[] arrayOfOutputTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(outputValues);
                   // Console.WriteLine("{0}\t{1}", item.GetType().Name, Marshal.SizeOf(item));
                }
            }
            Console.WriteLine(totalLengthInBytesOfRequestedData);

            Console.WriteLine("MyClass3------------------");
            MyClass3 cl3 = new MyClass3();
            tmp = (object)cl3;
            Type[] arrOutput3 = GetObjectPropertiesTypesArray(tmp);
            foreach (var item3 in arrOutput3)
            {
                Console.WriteLine(item3.ToString());
            }
            
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

           
            Console.ReadLine();
            /*ModbusRTUProtocol protM = new ModbusRTUProtocol();
            protM.Connect("COM8", timeout: 1500);

            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };
            bool bRetCode = protM.AddCRC(ref packet);

            protM.Disconnect();          
            Console.ReadLine();*/
           
        }      
        
    }
}
