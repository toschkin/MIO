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

        ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, object setValue);
        //ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, UInt16 setValue);
        //ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, Int16 setValue);

        ModbusErrorCode ForceMultipleCoils(Byte rtuAddress, UInt16 forceAddress, bool[] values);

        ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, object[] values, bool bigEndianOrder = false);
        //ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, Int16[] values);       
    }
}
