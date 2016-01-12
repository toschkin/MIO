using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Core
{
    public class Crc16
    {      
        /// <summary>
        /// Adds two bytes of CRC16 to array representing Modbus protocol data packet passed in argument
        /// </summary>
        /// <param name="packet">data packet to which we need to add CRC16</param>
        /// <param name="polynom">polynom by which to calculate CRC16</param>
        ///  <exception cref="System.ArgumentNullException">Thrown if packet is null</exception>
        ///  <exception cref="System.ArgumentOutOfRangeException">Thrown if argument length &lt; 2 or &gt; 254</exception>
        public static void AddCrc(ref Byte[] packet, UInt16 polynom = 0xA001)
        {
            if (packet == null)
                throw new ArgumentNullException();
            if ((packet.Length<3)||(packet.Length>254))
                throw new ArgumentOutOfRangeException();
        
            UInt16 crc = 0xFFFF;

            for (int pos = 0; pos < packet.Length; pos++)
            {
                crc ^= (UInt16)packet[pos];          // XOR byte into least sig. byte of crc
 
                for (int i = 8; i != 0; i--) 
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0) 
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= polynom;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }
            List<byte> listCrc = new List<byte>();
            listCrc.AddRange(packet);
            listCrc.Add(BitConverter.GetBytes(crc).ElementAt<Byte>(0));
            listCrc.Add(BitConverter.GetBytes(crc).ElementAt<Byte>(1));
            Array.Resize(ref packet, listCrc.Count);
            listCrc.ToArray().CopyTo(packet, 0);                                             
        }

        /// <summary>
        /// Checks CRC16 of Modbus protocol data packet passed in argument
        /// </summary>
        /// <param name="packet"> packet, which CRC we need to verify</param>
        /// <param name="polynom">polynom by which to calculate CRC16</param>
        /// <returns>true on success, otherwise false</returns>            
        public static bool CheckCrc(Byte[] packet, UInt16 polynom = 0xA001)
        {
            StringBuilder stb = new StringBuilder();
            for (int pos = 0; pos < packet.Length - 2; pos++)
            {
                stb.AppendFormat("{0:X2}", packet[pos]);
            }
            if (packet == null)
                return false;
            if ((packet.Length < 5) || (packet.Length > 256))
                return false;

            UInt16 crc = 0xFFFF;

            for (int pos = 0; pos < packet.Length-2; pos++)
            {
                crc ^= (UInt16)packet[pos];          // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }

            if((packet[packet.Length-2] == BitConverter.GetBytes(crc).ElementAt<Byte>(0))
               &&(packet[packet.Length-1] == BitConverter.GetBytes(crc).ElementAt<Byte>(1)))
                return true;
            
            return false;
        }              
    }
}