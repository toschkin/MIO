using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Modbus.Core
{
    /// <summary>
    /// Modbus protocol functions
    /// </summary>
    interface IModbus
    {      
        /// <summary>
        /// Implemented in inherited classes
        /// </summary>        
        ModbusErrorCode ReadHoldingRegisters<T>(Byte rtuAddress, UInt16 startAddress, UInt16 registersCount, ref T[] registerValues, bool reverseOrder=false);
        void AddCRC(ref Byte[] packet);
        bool CheckCRC(Byte[] packet);
        Byte[] MakePacket(Byte slaveAddress, Byte functionCode, UInt16 startAddress, UInt16 quantity);
        ModbusErrorCode CheckPacket(Byte[] packetRecieved, Byte slaveAddress, Byte functionCode, Int32 expectedPacketLength);
    }
}
