﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Core
{
    public static class ConversionHelper
    {       
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
        /// <summary>
        /// Converts bytes from array to single floating point number
        /// </summary>
        /// <param name="array">input array of bytes</param>
        /// <param name="index">index of element of array from which to start convertion</param>
        /// <param name="reverseOrder">if true, elements from array will be took in reverse order</param>
        /// <returns>converted single floating point number</returns>
        public static float ConvertBytesToFloat(byte[] array, int index, bool reverseOrder) 
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
      
    }
}
