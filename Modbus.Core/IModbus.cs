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
    public interface IModbus
    {      
        /// <summary>
        /// Implemented in inherited classes
        /// </summary>  
        ///       
        ModbusErrorCode ReadCoilStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues);
        ModbusErrorCode ReadInputStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues);
        ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder=false);
        ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder = false);
        ModbusErrorCode ForceSingleCoil(Byte rtuAddress, UInt16 forceAddress, bool setOn);
        ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, object setValue);       
        ModbusErrorCode ForceMultipleCoils(Byte rtuAddress, UInt16 forceAddress, bool[] values);
        ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, object[] values, bool bigEndianOrder = false);        
        ///overloads with lists in arguments
        ModbusErrorCode ReadCoilStatus(Byte rtuAddress, UInt16 startAddress, ref List<bool> statusesValues);
        ModbusErrorCode ReadInputStatus(Byte rtuAddress, UInt16 startAddress, ref List<bool> statusesValues);
        ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, bool bigEndianOrder = false);
        ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, bool bigEndianOrder = false);
        ModbusErrorCode ForceMultipleCoils(Byte rtuAddress, UInt16 forceAddress, List<bool> values);
        ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, List<object> values, bool bigEndianOrder = false);

        //overloads with indexing in object array
        ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false);
        ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false);
        ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false);
        ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false);
        ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, object[] values, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false);
        ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, List<object> values, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false);
        bool IsConnected { get; }
        Byte ReadRegistersPerQueryCapacity { get; set; }
        Byte WriteRegistersPerQueryCapacity { get; set; }
    }
}
