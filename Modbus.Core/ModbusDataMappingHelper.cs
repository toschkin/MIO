using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Modbus.Core
{
    public static class ModbusDataMappingHelper
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
                if (!IsNumericType(item.PropertyType))
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
                    if (!IsNumericType(item2.PropertyType))
                        throw new ArgumentException();
                    if (item2.PropertyType.IsPublic)
                        lstTypesOfClassMembers.Add(item2.PropertyType);
                }     
            }           
            return lstTypesOfClassMembers.ToArray<Type>();
        }
        /// <summary>
        /// Sets all public properties of the object obj to values from arrayValues
        /// </summary>
        /// <param name="obj">Destination object</param>
        /// <param name="arrayValues">Values to which this function sets properties of the object obj</param>
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
            else
                return false;
        }
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
    }    
}
