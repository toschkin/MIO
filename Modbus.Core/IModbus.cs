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
        ///       
        ModbusErrorCode ReadCoilStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] registerValues);
        ModbusErrorCode ReadInputStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] registerValues);
        ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder=false);
        ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder = false);

        ModbusErrorCode ForceSingleCoil(Byte rtuAddress, UInt16 forceAddress, bool setOn);
        
        ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, UInt16 setValue);
        ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, Int16 setValue);

        ModbusErrorCode ForceMultipleCoils(Byte functionNumber, Byte rtuAddress, UInt16 forceAddress, bool[] values);

        ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, UInt16[] values);
        ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, Int16[] values);

        /*void AddCRC(ref Byte[] packet);
        bool CheckCRC(Byte[] packet);
        Byte[] MakePacketForce(Byte slaveAddress, Byte functionCode, UInt16 startAddress, UInt16 quantity);
        ModbusErrorCode CheckPacket(Byte[] packetRecieved, Byte slaveAddress, Byte functionCode, Int32 expectedPacketLength);
        bool ProcessAnalogData(Byte[] rawPacketData, ref object[] outputValues);*/
    }
}
