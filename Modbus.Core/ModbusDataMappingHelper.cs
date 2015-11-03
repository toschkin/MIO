using System;
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
    public static class SizeofHelper
    {
        /// <summary>
        /// Calculates total size in bytes of all public properties of an object
        /// </summary>
        /// <param name="obj">object which properties size should be calculated</param>
        /// <returns>size in bytes of all public properties of an object</returns>
        public static UInt32 SizeOfPublicProperties(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            UInt32 totalLengthInBytes = 0;
            foreach (var field in obj.GetType().GetProperties())
            {
                if (field.PropertyType.IsPublic)	
                    totalLengthInBytes += (UInt32)Marshal.SizeOf(field.PropertyType);                
            }
            return totalLengthInBytes;
        }
        /// <summary>
        /// Calculates total size in bytes of all public properties of an array of objects
        /// </summary>
        /// <param name="array">array which total element's properties sizes should be calculated</param>
        /// <returns>size in bytes of all public properties of an object's in array</returns>
        public static UInt32 SizeOfPublicProperties(object[] array)
        {
            if (array == null)
                throw new ArgumentNullException();
            
            UInt32 totalLengthInBytes = 0;
            foreach (var element in array)
            {
                foreach (var field in element.GetType().GetProperties())
                {
                    if (field.PropertyType.IsPublic)
                        totalLengthInBytes += (UInt32)Marshal.SizeOf(field.PropertyType);
                }
            }            
            return totalLengthInBytes;
        }
    }

    public static class GetTypeHelper
    {
        /// <summary>
        /// Checks whether the type is numerical
        /// </summary>
        /// <param name="type">A type to check</param>
        /// <returns>true if type is numerical, false otherwise</returns>
        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
    
    public static class ModbusDataMappingHelper
    {
        /// <summary>
        /// Extracts values of object's public properties
        /// </summary>
        /// <param name="obj">object which properties values should be extracted</param>
        /// <returns>array of public properties values</returns>
        public static object[] GetObjectPropertiesValuesArray(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            List<object> lstTypesOfClassMembers = new List<object>();
            PropertyInfo[] membersOfClass = obj.GetType().GetProperties();
            foreach (var item in membersOfClass)
            {
                if (!GetTypeHelper.IsNumericType(item.PropertyType))
                    throw new ArgumentException();
                
                if ((item.PropertyType.IsPublic) && (item.GetIndexParameters().Length == 0))
                    lstTypesOfClassMembers.Add(item.GetValue(obj));
            }
            return lstTypesOfClassMembers.ToArray<object>();
        }
        /// <summary>
        /// Extracts types of object's public properties
        /// </summary>
        /// <param name="obj">object which properties should be extracted</param>
        /// <returns>array of public properties types</returns>
        public static Type[] GetObjectPropertiesTypeArray(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
                        
            List<Type> lstTypesOfClassMembers = new List<Type>();
            PropertyInfo[] membersOfClass = obj.GetType().GetProperties();
            foreach (var item in membersOfClass)
            {
                if (!GetTypeHelper.IsNumericType(item.PropertyType))
                    throw new ArgumentException();
                if (item.PropertyType.IsPublic)
                    lstTypesOfClassMembers.Add(item.PropertyType);
            }
            return lstTypesOfClassMembers.ToArray<Type>();
        }
        /// <summary>
        /// Extracts types of public properties of objects in array
        /// </summary>
        /// <param name="obj">array of objects which properties should be extracted</param>
        /// <returns>array of public properties types</returns>       
        public static Type[] GetObjectPropertiesTypeArray(object[] obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            List<Type> lstTypesOfClassMembers = new List<Type>();
            foreach (var item in obj)
            {
                if (item == null)
                    throw new ArgumentNullException();                

                PropertyInfo[] membersOfClass = item.GetType().GetProperties();
                foreach (var item2 in membersOfClass)
                {
                    if (!GetTypeHelper.IsNumericType(item2.PropertyType))
                        throw new ArgumentException();
                    if (item2.PropertyType.IsPublic)
                        lstTypesOfClassMembers.Add(item2.PropertyType);
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
        public static bool SetObjectPropertiesValuesFromArray(ref object obj, object[] arrayValues)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (arrayValues == null)
                throw new ArgumentNullException();

            if (obj.GetType().GetProperties().Length == arrayValues.Length)
            {
                //first we check that arrayValues has same value types with object properties
                int i = 0;
                foreach (var item in obj.GetType().GetProperties())
                {
                    if (item.PropertyType.IsPublic)
                    {
                        if (item.PropertyType == arrayValues[i].GetType())
                            i++;
                        else
                            return false;    
                    }                    
                }
                //setting values from arrayValues to corresponding properties  
                i = 0;
                foreach (var item in obj.GetType().GetProperties())
                {
                    if (item.PropertyType.IsPublic)                   
                        item.SetValue(obj, Convert.ChangeType(arrayValues[i], item.PropertyType));                   
                    i++;
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
                    value = ConversionHelper.ReverseBytes(BitConverter.ToUInt16(packetRawData, currentCursorPosition));
                    break;
                }
                case "Int16":
                {
                    value = ConversionHelper.ReverseBytes(BitConverter.ToInt16(packetRawData, currentCursorPosition));
                    break;
                }
                case "UInt32":
                {                    
                    value = ConversionHelper.ReverseBytes(BitConverter.ToUInt32(packetRawData, currentCursorPosition));
                    if (!bigEndianOrder)                   
                        value = (UInt32)((BitConverter.ToUInt16( BitConverter.GetBytes((UInt32)value) , 0) << 16) + BitConverter.ToUInt16(BitConverter.GetBytes((UInt32)value), 2));
                    break;
                }
                case "Int32":
                {                   
                    value = ConversionHelper.ReverseBytes(BitConverter.ToInt32(packetRawData, currentCursorPosition));                        
                    if (!bigEndianOrder)
                        value = (Int32)((BitConverter.ToUInt16(BitConverter.GetBytes((Int32)value), 0) << 16) + BitConverter.ToUInt16(BitConverter.GetBytes((Int32)value), 2));
                    break;
                }
                case "UInt64":
                {
                    value = ConversionHelper.ReverseBytes(BitConverter.ToUInt64(packetRawData, currentCursorPosition));
                    if (!bigEndianOrder)                    
                        value = ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 0) << 48) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 2) << 32) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 4) << 16) | (UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((UInt64)value), 6);                    
                    break;
                }
                case "Int64":
                {
                    value = ConversionHelper.ReverseBytes(BitConverter.ToInt64(packetRawData, currentCursorPosition));
                    if (!bigEndianOrder)
                        value = (Int64)(((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 0) << 48) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 2) << 32) | ((UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 4) << 16) | (UInt64)BitConverter.ToUInt16(BitConverter.GetBytes((Int64)value), 6));                    
                    break;
                }
                case "Single":
                {
                    value = ConversionHelper.ConvertBytesToFloat(packetRawData, currentCursorPosition, true);
                    if (!bigEndianOrder)
                        value = ConversionHelper.ConvertBytesToFloat(BitConverter.GetBytes((BitConverter.ToUInt16(BitConverter.GetBytes((Single)value), 0) << 16) + BitConverter.ToUInt16(BitConverter.GetBytes((Single)value), 2)), 0);
                    break;
                }
                case "Double":
                {
                    value = ConversionHelper.ConvertBytesToDouble(packetRawData, currentCursorPosition, true);
                    if (!bigEndianOrder)                        
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
                    if (!bigEndianOrder)
                    {
                        //0x81, 0x15, 0x7D, 0xE9, 0x10, 0xF4, 0x11, 0x22, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x9
                        // HI    LO                                                                      HI  LO
                        //    LS                                                                           MS                        
                        for (int i = currentCursorPosition; i < currentCursorPosition+16; i+=2)
                        {
                            Byte tmp = packetRawData[i];
                            packetRawData[i] = packetRawData[i + 1];
                            packetRawData[i + 1] = tmp;
                        }
                        value = ConversionHelper.ConvertBytesToDecimal(packetRawData, currentCursorPosition);
                    }
                    else
                    {
                        value = ConversionHelper.ConvertBytesToDecimal(packetRawData, currentCursorPosition, true);
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
    }    
}