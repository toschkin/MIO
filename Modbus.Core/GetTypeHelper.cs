using System;

namespace Modbus.Core
{
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
}