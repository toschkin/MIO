using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Modbus.Core
{
    public static class SizeofHelper
    {        
        /// <summary>
        /// Calculates total size in bytes of all public properties of an array of objects
        /// </summary>
        /// <param name="array">array which total element's properties sizes should be calculated</param>
        /// <returns>size in bytes of all public properties of an object's in array</returns>
        public static UInt32 SizeOfPublicPropertiesWithModbusAttribute(object[] array, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead, bool processSubproperties = false)
        {
            if (array == null)
                throw new ArgumentNullException();
            
            UInt32 totalLengthInBytes = 0;
            foreach (var element in array)
            {
                foreach (var field in OrderedPropertiesGetter.GetObjectPropertiesInDeclarationOrder(element))
                {
                    if ((field.CanWrite) &&
                        (field.GetCustomAttributes(typeof (ModbusPropertyAttribute), false).Length != 0))
                    {
                        foreach (var attr in field.GetCustomAttributes(typeof(ModbusPropertyAttribute), false))
                        {
                            if (((ModbusPropertyAttribute)attr).Access >= accessType)
                            {
                                totalLengthInBytes += (UInt32)Marshal.SizeOf(field.PropertyType);
                                break;
                            } 
                        }                                                    
                    }
                    else
                    {
                        if ((processSubproperties) && (field.CanWrite))
                        {
                            if (!field.PropertyType.IsValueType && !field.PropertyType.IsEnum &&
                            field.PropertyType == typeof(String))
                            {
                                Type typeSource = field.GetType();
                                object objTarget = Activator.CreateInstance(typeSource);
                                totalLengthInBytes += SizeOfPublicPropertiesWithModbusAttribute(objTarget, accessType);
                            }
                        }
                    }  
                }
            }            
            return totalLengthInBytes;
        }

        /// <summary>
        /// Calculates total size in bytes of all public properties of an object
        /// </summary>
        /// <param name="obj">object which properties size should be calculated</param>
        /// <returns>size in bytes of all public properties of an object</returns>
        public static UInt32 SizeOfPublicPropertiesWithModbusAttribute(object obj, ModbusRegisterAccessType accessType = ModbusRegisterAccessType.AccessRead, bool processSubproperties = false)
        {
            if (obj is List<object>)
                return SizeOfPublicPropertiesWithModbusAttribute(((List<object>)obj).ToArray(), accessType);

            if (obj == null)
                throw new ArgumentNullException();

            UInt32 totalLengthInBytes = 0;
            foreach (var field in OrderedPropertiesGetter.GetObjectPropertiesInDeclarationOrder(obj))
            {

                if ((field.CanWrite) &&
                    (field.GetCustomAttributes(typeof(ModbusPropertyAttribute), false).Length != 0))
                {
                    foreach (var attr in field.GetCustomAttributes(typeof(ModbusPropertyAttribute), false))
                    {
                        if (((ModbusPropertyAttribute)attr).Access >= accessType)
                        {
                            totalLengthInBytes += (UInt32)Marshal.SizeOf(field.PropertyType);
                            break;
                        }
                    }
                }
                else
                {
                    if ((processSubproperties)&&(field.CanWrite))
                    {
                        if (!field.PropertyType.IsValueType && !field.PropertyType.IsEnum &&
                        field.PropertyType == typeof(String))
                        {
                            Type typeSource = field.GetType();
                            object objTarget = Activator.CreateInstance(typeSource);
                            totalLengthInBytes += SizeOfPublicPropertiesWithModbusAttribute(objTarget, accessType);
                        }    
                    }                    
                }
            }
            return totalLengthInBytes;
        }
    }
}