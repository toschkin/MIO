using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Modbus.Core
{
    public static class ModbusDataMappingHelper
    {
        public static bool IsNumericType(Type o)
        {
            switch (Type.GetTypeCode(o))
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
                lstTypesOfClassMembers.Add(item.PropertyType);
            }
            return lstTypesOfClassMembers.ToArray<Type>();
        }
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
                    lstTypesOfClassMembers.Add(item2.PropertyType);
                }     
            }           
            return lstTypesOfClassMembers.ToArray<Type>();
        }
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
    }    
}
