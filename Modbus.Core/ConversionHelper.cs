using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Core
{
    //From http://social.technet.microsoft.com/wiki/contents/articles/19055.convert-system-decimal-to-and-from-byte-arrays-vb-c.aspx
    public class BitConverterEx
    {
        public static byte[] GetBytes(decimal dec)
        {
            //Load four 32 bit integers from the Decimal.GetBits function
            Int32[] bits = decimal.GetBits(dec);
            //Create a temporary list to hold the bytes
            List<byte> bytes = new List<byte>();
            //iterate each 32 bit integer
            foreach (Int32 i in bits)
            {
                //add the bytes of the current 32bit integer
                //to the bytes list
                bytes.AddRange(BitConverter.GetBytes(i));
            }
            //return the bytes list as an array
            return bytes.ToArray();
        }
        public static decimal ToDecimal(byte[] bytes,int offset)
        {
            if (bytes == null)
                throw new ArgumentNullException();
            //check that it is even possible to convert the array
            if (offset > bytes.Length - 16)
                throw new ArgumentException();
            return new decimal(
                                BitConverter.ToInt32(bytes, offset),
                                BitConverter.ToInt32(bytes, offset + 4),
                                BitConverter.ToInt32(bytes, offset + 8),
                                bytes[offset + 15] == (Byte)128,
                                bytes[offset + 14]);
        }
    }
    public static class ConversionHelper
    {
        // reverse byte order (16-bit)
        public static Int16 ReverseBytes(Int16 value)
        {
            return (Int16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        // reverse byte order (32-bit)
        public static Int32 ReverseBytes(Int32 value)
        {
            UInt32 temp = unchecked((UInt32)value);
            temp = ReverseBytes(temp);
            return unchecked((Int32)temp); ;
        }
        
        // reverse byte order (16-bit)
        public static UInt16 ReverseBytes(UInt16 value)
        {
          return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
      
        // reverse byte order (32-bit)
        public static UInt32 ReverseBytes(UInt32 value)
        {
          return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                 (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
      
        // reverse byte order (64-bit)
        public static UInt64 ReverseBytes(UInt64 value)
        {
          return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                 (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                 (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                 (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
        public static Int64 ReverseBytes(Int64 value)
        {
            UInt64 temp = unchecked((UInt64)value);
            temp = ReverseBytes(temp);
            return unchecked((Int64)temp); ;
        }
        /// <summary>
        /// Converts bytes from array to single floating point number
        /// </summary>
        /// <param name="array">input array of bytes</param>
        /// <param name="index">index of element of array from which to start convertion</param>
        /// <param name="reverseOrder">if true, elements from array will be took in reverse order</param>
        /// <returns>converted single floating point number</returns>
        public static Single ConvertBytesToFloat(byte[] array, int index, bool reverseOrder = false) 
        {            
            if (index > array.Length - 4)
                throw new ArgumentOutOfRangeException();

            if (array == null)
                throw new ArgumentNullException();
            
            float floatValue = 0.0f;

            if (reverseOrder)
                Array.Reverse(array, index, 4);
           
            floatValue = BitConverter.ToSingle(array, index);
            
            return floatValue;
        }

        public static Double ConvertBytesToDouble(byte[] array, int index, bool reverseOrder = false)
        {
            if (index > array.Length - 8)
                throw new ArgumentOutOfRangeException();

            if (array == null)
                throw new ArgumentNullException();

            Double doubleValue = 0.0;

            if (reverseOrder)
                Array.Reverse(array, index, 8);

            doubleValue = BitConverter.ToDouble(array, index);

            return doubleValue;
        }

        public static Decimal ConvertBytesToDecimal(byte[] array, int index, bool reverseOrder = false)
        {
            if (index > array.Length - 16)
                throw new ArgumentOutOfRangeException();

            if (array == null)
                throw new ArgumentNullException();

            Decimal decimalValue = 0.0m;

            if (reverseOrder)
                Array.Reverse(array, index, 16);

            decimalValue = BitConverterEx.ToDecimal(array, index);

            return decimalValue;
        }
    }
}
