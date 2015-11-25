using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace Modbus.Core
{
    public static class ModbusDataMappingHelper
    {
        /// <summary>
        /// Extracts values of object's public properties
        /// </summary>
        /// <param name="obj">object which properties values should be extracted</param>
        /// <returns>array of public properties values</returns>
        public static object[] GetObjectModbusPropertiesValuesArray(object obj, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead)
        {
            if (obj == null)
                throw new ArgumentNullException();

            List<object> lstTypesOfClassMembers = new List<object>();
            PropertyInfo[] membersOfClass = obj.GetType().GetProperties();
            foreach (var item in membersOfClass)
            {
                if (GetTypeHelper.IsNumericType(item.PropertyType))
                {
                    if ((item.CanWrite) && (item.GetIndexParameters().Length == 0)
                        && (item.GetCustomAttributes(typeof (ModbusPropertyAttribute), false).Length != 0))
                    {
                        foreach (var attr in item.GetCustomAttributes(typeof(ModbusPropertyAttribute), false))
                        {
                            if (((ModbusPropertyAttribute)attr).Access >= accessType)
                            {
                                lstTypesOfClassMembers.Add(item.GetValue(obj));    
                                break;
                            }
                        }                        
                    }                        
                }                
            }
            return lstTypesOfClassMembers.ToArray<object>();
        }
        /// <summary>
        /// Extracts types of object's public properties
        /// </summary>
        /// <param name="obj">object which properties should be extracted</param>
        /// <returns>array of public properties types</returns>
        public static Type[] GetObjectModbusPropertiesTypeArray(object obj, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead)
        {
            if (obj == null)
                throw new ArgumentNullException();
                        
            List<Type> lstTypesOfClassMembers = new List<Type>();
            PropertyInfo[] membersOfClass = obj.GetType().GetProperties();
            foreach (var item in membersOfClass)
            {
                if (GetTypeHelper.IsNumericType(item.PropertyType))
                {
                    if ((item.CanWrite) &&
                        (item.GetCustomAttributes(typeof (ModbusPropertyAttribute), false).Length != 0))
                    {
                        foreach (var attr in item.GetCustomAttributes(typeof(ModbusPropertyAttribute), false))
                        {
                            if (((ModbusPropertyAttribute)attr).Access >= accessType)
                            {
                                lstTypesOfClassMembers.Add(item.PropertyType);
                                break;
                            }
                        } 
                    }
                    
                }                
            }
            return lstTypesOfClassMembers.ToArray<Type>();
        }
        /// <summary>
        /// Extracts types of public properties of objects in array
        /// </summary>
        /// <param name="obj">array of objects which properties should be extracted</param>
        /// <returns>array of public properties types</returns>       
        public static Type[] GetObjectModbusPropertiesTypeArray(object[] obj, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead)
        {
            if (obj == null)
                throw new ArgumentNullException();
            List<Type> lstTypesOfClassMembers = new List<Type>();
            foreach (var item in obj)
            {
                if (item == null)
                    throw new ArgumentNullException();                

                PropertyInfo[] membersOfClass = item.GetType().GetProperties();
                foreach (var property in membersOfClass)
                {
                    if (GetTypeHelper.IsNumericType(property.PropertyType))
                    {
                        if ((property.CanWrite) &&
                            (property.GetCustomAttributes(typeof (ModbusPropertyAttribute), false).Length != 0))
                        {
                            foreach (var attr in property.GetCustomAttributes(typeof(ModbusPropertyAttribute), false))
                            {
                                if (((ModbusPropertyAttribute)attr).Access >= accessType)
                                {
                                    lstTypesOfClassMembers.Add(property.PropertyType);    
                                    break;
                                }
                            } 
                        }

                    }                    
                }     
            }           
            return lstTypesOfClassMembers.ToArray<Type>();
        }
        /// <summary>
        /// Sets all public properties of the object array to values from arrayValues
        /// </summary>
        /// <param name="obj">Destination object</param>
        /// <param name="arrayValues">Values to which this function sets properties of the object array</param>
        /// <returns>true on success, false otherwise</returns>
        /// <remarks>Values in arrayValues should be placed sequentionaly in the same order with object's properties definitions</remarks>
        public static bool SetObjectPropertiesValuesFromArray(ref object obj, object[] arrayValues, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (arrayValues == null)
                throw new ArgumentNullException();

            if (obj.GetType().GetProperties().Length >= arrayValues.Length)
            {
                //first we check that arrayValues has same value types with object properties
                int i = 0;
                foreach (var item in obj.GetType().GetProperties())
                {
                    if ((item.CanWrite) && (item.GetCustomAttributes(typeof(ModbusPropertyAttribute), false).Length != 0))
                    {
                        foreach (var attr in item.GetCustomAttributes(typeof(ModbusPropertyAttribute), false))
                        {
                            if (((ModbusPropertyAttribute)attr).Access >= accessType)
                            {
                                if (item.PropertyType == arrayValues[i].GetType())
                                {                                    
                                    item.SetValue(obj, Convert.ChangeType(arrayValues[i], item.PropertyType));
                                    i++;
                                }                                    
                                else
                                    return false;    
                                break;
                            }
                        }                         
                    }                    
                }               
                return true;
            }            
            return false;
        }
        /// <summary>
        /// Converts raw Modbus packet data to numeric type value
        /// </summary>
        /// <param name="packetRawData">raw data array from Modbus packet</param>
        /// <param name="currentCursorPosition">current position in data array (will be incremented by size of object inside the method call)</param>
        /// <param name="value">numeric object to which extract value from data array</param>
        /// <param name="bigEndianOrder">if true modbus registers are processed to 32bit(or higher) value in big endian order: first register - high 16bit, second - low 16bit</param>
        public static void ExtractValueFromArrayByType(Byte[] packetRawData, ref Int32 currentCursorPosition, ref object value, bool bigEndianOrder = false)
        {
            if (!GetTypeHelper.IsNumericType(value.GetType())) 
                throw  new ArgumentException();
            if (packetRawData == null)
                throw new ArgumentNullException();
            int valueSize = Marshal.SizeOf(value);
            if ((currentCursorPosition < 0) || (currentCursorPosition > packetRawData.Length - valueSize))
                throw new ArgumentOutOfRangeException();
            switch (value.GetType().Name)
            {
                case "Byte":
                {
                    value = packetRawData[currentCursorPosition];
                    break;
                }
                case "SByte":
                {
                    value = unchecked((SByte)(packetRawData[currentCursorPosition]));
                    break;
                }
                case "UInt16":
                {
                    value = BitConverter.ToUInt16(packetRawData, currentCursorPosition);
                    break;
                }
                case "Int16":
                {
                    value = BitConverter.ToInt16(packetRawData, currentCursorPosition);
                    break;
                }
                case "UInt32":
                {                    
                    value = BitConverter.ToUInt32(packetRawData, currentCursorPosition);
                    if (bigEndianOrder)                   
                        value = (UInt32)((BitConverter.ToUInt16( BitConverter.GetBytes((UInt32)value) , 0) << 16) + BitConverter.ToUInt16(BitConverter.GetBytes((UInt32)value), 2));
                    break;
                }
                case "Int32":
                {                   
                    value = BitConverter.ToInt32(packetRawData, currentCursorPosition);                        
                    if (bigEndianOrder)
                        value = (Int32)((BitConverter.ToUInt16(BitConverter.GetBytes((Int32)value), 0) << 16) + BitConverter.ToUInt16(BitConverter.GetBytes((Int32)value), 2));
                    break;
                }
                case "UInt64":
                {
                    value = BitConverter.ToUInt64(packetRawData, currentCursorPosition);
                    if (bigEndianOrder)                    
                        value = ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 0) << 48) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 2) << 32) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 4) << 16) | (UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 6);                    
                    break;
                }
                case "Int64":
                {
                    value = BitConverter.ToInt64(packetRawData, currentCursorPosition);
                    if (bigEndianOrder)
                        value = (Int64)(((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 0) << 48) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 2) << 32) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 4) << 16) | (UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 6));                    
                    break;
                }
                case "Single":
                {
                    value = ConversionHelper.ConvertBytesToFloat(packetRawData, currentCursorPosition, false);
                    if (bigEndianOrder)
                        value = ConversionHelper.ConvertBytesToFloat(BitConverter.GetBytes((BitConverter.ToUInt16(BitConverter.GetBytes((Single)value), 0) << 16) + BitConverter.ToUInt16(BitConverter.GetBytes((Single)value), 2)), 0);
                    break;
                }
                case "Double":
                {
                    value = ConversionHelper.ConvertBytesToDouble(packetRawData, currentCursorPosition, false);
                    if (bigEndianOrder)                        
                        value =
                            ConversionHelper.ConvertBytesToDouble(
                                BitConverter.GetBytes(
                                    ((UInt64) BitConverter.ToUInt16(BitConverter.GetBytes((Double) value), 0) << 48) |
                                    ((UInt64) BitConverter.ToUInt16(BitConverter.GetBytes((Double) value), 2) << 32) |
                                    ((UInt64) BitConverter.ToUInt16(BitConverter.GetBytes((Double) value), 4) << 16) |
                                    (UInt64) BitConverter.ToUInt16(BitConverter.GetBytes((Double) value), 6)), 0);
                    break;
                }
                case "Decimal":
                {
                    value = ConversionHelper.ConvertBytesToDecimal(packetRawData, currentCursorPosition, false);
             
                    if (bigEndianOrder)
                    {
                        //0x81, 0x15, 0x7D, 0xE9, 0x10, 0xF4, 0x11, 0x22, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x9
                        // HI    LO                                                                      HI  LO
                        //    LS                                                                           MS                        
                        ReverseWordsInBlocksOfByteArray(ref packetRawData, currentCursorPosition, 16, 2);
                        value = ConversionHelper.ConvertBytesToDecimal(packetRawData, currentCursorPosition);
                    }                   
                    break;
                }
            }
            currentCursorPosition += valueSize;
        }
        /// <summary>
        /// Creates an array of objects given by their types (only numeric types accepted)
        /// </summary>
        /// <param name="arrayOfTypes">Array of types from which will be created returned array</param>
        /// <returns>Array of created objects</returns>
        /// <remarks>Objects in output array are intialized each to its type default value</remarks>
        public static object[] CreateValuesArrayFromTypesArray(Type[] arrayOfTypes)
        {
            if (arrayOfTypes == null)
                throw new ArgumentNullException();

            List<object> listOfElem = new List<object>();
            for (int i = 0; i < arrayOfTypes.Length; i++)
            {
                if (!GetTypeHelper.IsNumericType(arrayOfTypes[i]))
                    throw new ArgumentException();

                if (arrayOfTypes[i].Name == "Byte")
                    listOfElem.Add(new Byte());
                if (arrayOfTypes[i].Name == "SByte")
                    listOfElem.Add(new SByte());
                if (arrayOfTypes[i].Name == "Int16")
                    listOfElem.Add(new Int16());
                if (arrayOfTypes[i].Name == "UInt16")
                    listOfElem.Add(new UInt16());
                if (arrayOfTypes[i].Name == "Int32")
                    listOfElem.Add(new Int32());
                if (arrayOfTypes[i].Name == "UInt32")
                    listOfElem.Add(new UInt32());
                if (arrayOfTypes[i].Name == "Int64")
                    listOfElem.Add(new Int64());
                if (arrayOfTypes[i].Name == "UInt64")
                    listOfElem.Add(new UInt64());
                if (arrayOfTypes[i].Name == "Single")
                    listOfElem.Add(new Single());
                if (arrayOfTypes[i].Name == "Double")
                    listOfElem.Add(new Double());
                if (arrayOfTypes[i].Name == "Decimal")
                    listOfElem.Add(new Decimal()); 
            }
            return listOfElem.ToArray<object>();
        }

        public static void ConvertObjectValueAndAppendToArray(ref Byte[] arrayOfBytes, object obj, bool bigEndianOrder = false)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if ((obj.GetType().IsValueType)&&(GetTypeHelper.IsNumericType(obj.GetType()))) //here we will process simple (only numeric) types
            {
                Array.Resize(ref arrayOfBytes, (arrayOfBytes==null?0:arrayOfBytes.Length) + Marshal.SizeOf(obj));

                switch (obj.GetType().Name)
                {
                    case "Byte":
                    {
                        arrayOfBytes[arrayOfBytes.Length - Marshal.SizeOf(obj)] = (Byte) obj;
                        break;
                    }
                    case "SByte":
                    {
                        arrayOfBytes[arrayOfBytes.Length - Marshal.SizeOf(obj)] = unchecked((Byte)((SByte)obj));
                        break;
                    }
                    case "UInt16":
                    {
                        for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj),k=0; i < arrayOfBytes.Length; i++,k++)
                        {
                            arrayOfBytes[i] = BitConverter.GetBytes((UInt16) obj).ElementAt<Byte>(k);
                        }                            
                        break;
                    }
                    case "Int16":
                    {
                        for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj),k=0; i < arrayOfBytes.Length; i++,k++)
                        {
                            arrayOfBytes[i] = BitConverter.GetBytes((Int16) obj).ElementAt<Byte>(k);
                        } 
                        break;
                    }
                    case "UInt32":
                        {
                            for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj), k = 0; i < arrayOfBytes.Length; i++, k++)
                            {
                                arrayOfBytes[i] = BitConverter.GetBytes((UInt32)obj).ElementAt<Byte>(k);
                            }                            
                            break;
                        }
                    case "Int32":
                        {
                            for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj), k = 0; i < arrayOfBytes.Length; i++, k++)
                            {
                                arrayOfBytes[i] = BitConverter.GetBytes((Int32)obj).ElementAt<Byte>(k);
                            } 
                            break;
                        }
                    case "UInt64":
                        {
                            for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj), k = 0; i < arrayOfBytes.Length; i++, k++)
                            {
                                arrayOfBytes[i] = BitConverter.GetBytes((UInt64)obj).ElementAt<Byte>(k);
                            } 
                            break;
                        }
                    case "Int64":
                        {
                            for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj), k = 0; i < arrayOfBytes.Length; i++, k++)
                            {
                                arrayOfBytes[i] = BitConverter.GetBytes((Int64)obj).ElementAt<Byte>(k);
                            } 
                            break;
                        }
                    case "Single":
                        {
                            for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj), k = 0; i < arrayOfBytes.Length; i++, k++)
                            {
                                arrayOfBytes[i] = BitConverter.GetBytes((Single)obj).ElementAt<Byte>(k);
                            } 
                            break;
                        }
                    case "Double":
                        {
                            for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj), k = 0; i < arrayOfBytes.Length; i++, k++)
                            {
                                arrayOfBytes[i] = BitConverter.GetBytes((Double)obj).ElementAt<Byte>(k);
                            } 
                            break;
                        }
                    case "Decimal":
                        {
                            for (int i = arrayOfBytes.Length - Marshal.SizeOf(obj), k = 0; i < arrayOfBytes.Length; i++, k++)
                            {
                                arrayOfBytes[i] = BitConverterEx.GetBytes((Decimal)obj).ElementAt<Byte>(k);
                            } 
                            break;
                        }
                }
                if (bigEndianOrder && Marshal.SizeOf(obj) > 2)
                    ReverseWordsInBlocksOfByteArray(ref arrayOfBytes,
                        arrayOfBytes.Length - Marshal.SizeOf(obj), (Byte)Marshal.SizeOf(obj), 2);
            }
            else            
                throw new ArgumentException();
            
        }

        public static void ReverseWordsInBlocksOfByteArray(ref byte[] array, int startIndex, byte blockSize, byte swapSeed)
        {
            if (array == null)
                throw new ArgumentNullException();
            if (startIndex + blockSize > array.Length)
                throw new ArgumentException("Out of array bounds");

            if (blockSize % swapSeed != 0)
                throw new ArgumentException(String.Format("blockSize (={0}) must be divisible by {1}", blockSize, swapSeed * 2));

            for (int i = startIndex, k = startIndex + blockSize - swapSeed; k>i; i += swapSeed, k -= swapSeed)
            {
                Byte[] tempBytes = new byte[swapSeed];
                for (int j = 0; j < tempBytes.Length; j++)
                {
                    tempBytes[j] = array[i + j];
                    array[i + j] = array[k + j];
                    array[k + j] = tempBytes[j];
                }
            }
        }
       
        public static void SwapBytesInWordsInByteArray(ref byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException();
            for (int i = 0; i < array.Length; i+=2)
            {
                Byte tempByte = array[i];
                if (i + 1 < array.Length)
                {
                    array[i] = array[i + 1];
                    array[i + 1] = tempByte;
                }
            }
        }

        public static Byte GetModbusObjectsHighestByte(object obj, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead)
        {
            if (obj == null)
                throw new ArgumentNullException();
                        
            if (!GetTypeHelper.IsNumericType(obj.GetType()))
            {
                for (int i = obj.GetType().GetProperties().Length-1; i >=0; i--)
                {
                    if ((obj.GetType().GetProperties()[i].CanWrite) &&
                       (obj.GetType().GetProperties()[i].GetCustomAttributes(typeof(ModbusPropertyAttribute), false).Length != 0))
                    {
                        foreach (var attr in obj.GetType().GetProperties()[i].GetCustomAttributes(typeof(ModbusPropertyAttribute), false))
                        {
                            if (((ModbusPropertyAttribute)attr).Access >= accessType)
                            {
                                obj = obj.GetType().GetProperties()[i].GetValue(obj);
                                break;
                            }
                        }
                        break;
                    }
                }                
            } 
          
            switch (obj.GetType().Name)
            {
                case "Byte":
                    {
                        return (Byte)obj;
                    }
                case "SByte":
                    {
                        return unchecked((Byte)((SByte)obj));
                    }
                case "UInt16":
                    {
                        return BitConverter.GetBytes((UInt16)obj).ElementAt<Byte>(BitConverter.GetBytes((UInt16)obj).Length - 1);
                    }
                case "Int16":
                    {
                        return BitConverter.GetBytes((Int16)obj).ElementAt<Byte>(BitConverter.GetBytes((Int16)obj).Length - 1);
                    }
                case "UInt32":
                    {
                        return BitConverter.GetBytes((UInt32)obj).ElementAt<Byte>(BitConverter.GetBytes((UInt32)obj).Length - 1);
                    }
                case "Int32":
                    {
                        return BitConverter.GetBytes((Int32)obj).ElementAt<Byte>(BitConverter.GetBytes((Int32)obj).Length - 1);
                    }
                case "UInt64":
                    {
                        return BitConverter.GetBytes((UInt64)obj).ElementAt<Byte>(BitConverter.GetBytes((UInt64)obj).Length - 1);
                    }
                case "Int64":
                    {
                        return BitConverter.GetBytes((Int64)obj).ElementAt<Byte>(BitConverter.GetBytes((Int64)obj).Length - 1);
                    }
                case "Single":
                    {
                        return BitConverter.GetBytes((Single)obj).ElementAt<Byte>(BitConverter.GetBytes((Single)obj).Length - 1);
                    }
                case "Double":
                    {
                        return BitConverter.GetBytes((Double)obj).ElementAt<Byte>(BitConverter.GetBytes((Double)obj).Length - 1);
                    }
                case "Decimal":
                    {
                        return BitConverterEx.GetBytes((Decimal)obj).ElementAt<Byte>(BitConverterEx.GetBytes((Decimal)obj).Length - 1);
                    }
                default:
                    {
                        throw new ArgumentException();
                    }
            }  
        }

        public static void ConvertObjectsVaulesToRegisters(object[] values, uint startIndex, uint objectsCount,
            bool bigEndianOrder, out ushort[] forcedValues, out Type firstElementType, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead)
        {
            //array of output 16bit values
            forcedValues = null;

            //only for determine if we need to preset multiple coils
            firstElementType = values[0].GetType();
            bool[] boolValues = null;
            //if firstElementType is boolean then all elements of array are considered to be boolean type also
            if (firstElementType == typeof(bool))
            {
                if (startIndex + objectsCount > 2000) //can force maximum 2000 coils                
                    throw new ArgumentException();

                Array.Resize(ref forcedValues, (int)(startIndex + objectsCount + 15) / 16);
                Array.Resize(ref boolValues, (int)(startIndex + objectsCount));
            }
            Byte[] tempArrayOfBytes = null;            
            int i = 0;
            //foreach (var value in values)
            for (UInt32 regVal = startIndex; regVal < startIndex + objectsCount; regVal++)
            {
                //if arrayType is bool - all elements must be bool
                if ((firstElementType == typeof(bool)) && (firstElementType != values[regVal].GetType()))
                    throw new ArgumentException();

                if (firstElementType == typeof(bool))
                    boolValues[i] = (bool)values[regVal];

                //if another type 
                if (values[regVal].GetType().IsValueType) //here we will process simple (only numeric) types
                {
                    if (GetTypeHelper.IsNumericType(values[regVal].GetType()))
                    {                       
                        ConvertObjectValueAndAppendToArray(ref tempArrayOfBytes, values[regVal],
                            bigEndianOrder);
                    }
                    else if (values[regVal].GetType() != typeof (bool))
                        throw new ArgumentException();

                }
                else //here we will process properties (only numeric) from complex (class) types 
                {                                       
                    object[] arrayOfObjectPropsValues =
                        GetObjectModbusPropertiesValuesArray(values[regVal], accessType);

                    for (int k = 0; k < arrayOfObjectPropsValues.Length; k++)
                    {
                        ConvertObjectValueAndAppendToArray(ref tempArrayOfBytes,
                            arrayOfObjectPropsValues[k],
                            bigEndianOrder);
                    }
                }
                i++;
            }

            if (firstElementType == typeof(bool))
            {
                Byte[] temp = new Byte[forcedValues.Length * 2];
                new BitArray(boolValues).CopyTo(temp, 0);
                Buffer.BlockCopy(temp, 0, forcedValues, 0, temp.Length);
            }
            else
            {
                Array.Resize(ref forcedValues, (tempArrayOfBytes.Length + 1) / 2);
                Buffer.BlockCopy(tempArrayOfBytes, 0, forcedValues, 0, tempArrayOfBytes.Length);
            }            
        }

        /// <summary>
        /// Fills array of bool values from modbus packet
        /// </summary>
        /// <param name="rawPacketData">raw data extracted from modbus packet</param>
        /// <param name="outputValues">array of boolean values to fill</param>        
        /// <returns>true on success, false otherwise</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>        
        /// <remarks>outputValues can contain numeric types and classes or structs with public numeric properties</remarks>
        public static void ProcessDiscreetData(Byte[] rawPacketData, ref bool[] outputValues)
        {
            if (rawPacketData == null)
                throw new ArgumentNullException();

            if (outputValues.Length > rawPacketData.Length * 8)
                throw new ArgumentOutOfRangeException();

            int deltaResize = rawPacketData.Length * 8 - outputValues.Length;

            Array.Resize<bool>(ref outputValues, rawPacketData.Length * 8);
            BitArray bitsOfRawPacketData = new BitArray(rawPacketData);
            bitsOfRawPacketData.CopyTo(outputValues, 0);

            Array.Resize<bool>(ref outputValues, outputValues.Length - deltaResize);
        }

        /// <summary>
        /// Fills array of objects with values from array of raw data extracted from modbus packet
        /// </summary>
        /// <param name="rawPacketData">raw data extracted from modbus packet</param>
        /// <param name="outputValues">array of objects to fill</param>
        /// <param name="startIndex">start position in array to which objects will be filled</param>
        /// <param name="objectsCount">count of objects that will be filled</param>
        /// <param name="bigEndianOrder">if true modbus registers are processed to 32bit(or higher) values in big endian order: first register - high 16bit, second - low 16bit</param>
        /// <returns>true on success, false otherwise</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.NullReferenceException"></exception>
        /// <remarks>outputValues can contain numeric types and classes or structs with public numeric properties</remarks>
        public static void ProcessAnalogData(Byte[] rawPacketData, ref object[] outputValues, UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            if (rawPacketData == null)
                throw new ArgumentNullException();
            if (startIndex + objectsCount > outputValues.Length)
                throw new ArgumentOutOfRangeException();

            UInt32 totalLengthInBytesOfRequestedData = 0;
            int currentIndexInPacketData = 0;

            for (UInt32 val = startIndex; val < startIndex + objectsCount; val++)
            {
                if (outputValues[val] == null)
                    throw new NullReferenceException();

                //counting total size
                if (outputValues[val].GetType().IsValueType)//here we will process simple (only numeric) types
                {
                    if (GetTypeHelper.IsNumericType(outputValues[val].GetType()))
                    {
                        totalLengthInBytesOfRequestedData += (UInt32)Marshal.SizeOf(outputValues[val]);
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            throw new ArgumentOutOfRangeException();                        
                    }
                    else
                        throw new ArgumentException("Neither numeric nor class value in outputValues array");
                }
                else//here we will process properties (only numeric) from complex (class) types 
                {                    
                    totalLengthInBytesOfRequestedData += SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(outputValues[val]);
                    if (totalLengthInBytesOfRequestedData > 0)
                    {
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            throw new ArgumentOutOfRangeException();                        
                    }
                }
                //processing data
                ProcessDataToObject(rawPacketData, ref outputValues[val], bigEndianOrder,ref currentIndexInPacketData);
            }
        }

        private static void ProcessDataToObject(byte[] rawPacketData, ref object outputValue, bool bigEndianOrder,
            ref int currentIndexInPacketData)
        {            
            if (outputValue.GetType().IsValueType) //here we will process simple (only numeric) types
            {
                if (GetTypeHelper.IsNumericType(outputValue.GetType()))
                {
                    ExtractValueFromArrayByType(rawPacketData,
                        ref currentIndexInPacketData, ref outputValue, bigEndianOrder);
                }
            }
            else //here we will process properties (only numeric) from complex (class) types 
            {
                
                Type[] arrayOfOutputTypes = GetObjectModbusPropertiesTypeArray(outputValue);
                object[] arrayOfValuesForObject =
                    CreateValuesArrayFromTypesArray(arrayOfOutputTypes);

                for (int i = 0; i < arrayOfValuesForObject.Length; i++)
                {
                    ExtractValueFromArrayByType(rawPacketData,
                        ref currentIndexInPacketData, ref arrayOfValuesForObject[i], bigEndianOrder);
                }
                SetObjectPropertiesValuesFromArray(
                    ref outputValue, arrayOfValuesForObject);
            }            
        }
    }    
}