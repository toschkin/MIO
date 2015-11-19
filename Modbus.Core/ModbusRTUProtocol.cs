using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Modbus.Core
{
    /// <summary>
    /// Communication with devices using Modbus RTU protocol
    /// </summary>
    public class ModbusRtuProtocol : RawSerialProtocol, IModbus
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ModbusRtuProtocol()
        {
            _writeRegistersPerQueryCapacity = 125;
            _readRegistersPerQueryCapacity = 125;
        }

        #region Logging
        /// <summary>
        /// Exception saving
        /// </summary>
        private SaveException _logExceptionModbusRtu;
        /// <summary>
        /// Adds logger to list of exceptions saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void SaveException(Exception message);</param>
        public void AddExceptionsLogger(SaveException logger)
        {
            _logExceptionModbusRtu += logger;
            LogExceptionRsp += logger;
        }
        /// <summary>
        /// Removes logger from current list of exceptions saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void SaveException(Exception message);</param>
        public void RemoveExceptionsLogger(SaveException logger)
        {
            _logExceptionModbusRtu += logger;
            LogExceptionRsp -= logger;
        }
        #endregion

        /// <summary>
        /// Reads statuses of output coils into array of boolean values
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>
        /// <param name="statusesValues">array of returned values</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads coil statuses from modbus slave 1 starting at address 0 and converts into boolean array of size 4.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// bool[] arrayOfCoilStatuses = { false, false, false, false};
        /// prot.ReadCoilStatus(1, 0, ref arrayOfCoilStatuses);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadCoilStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues)
        {
            return ReadStatuses(1, rtuAddress, startAddress, ref statusesValues);
        }
        /// <summary>
        /// Reads statuses of output coils into list of boolean values
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>
        /// <param name="statusesValues">list of returned values</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads coil statuses from modbus slave 1 starting at address 0 and converts into boolean list of size 3.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// List&lt;bool&gt; listOfCoilStatuses = new List&lt;bool&gt;();
        /// listOfCoilStatuses.Add(false);
        /// listOfCoilStatuses.Add(false);
        /// listOfCoilStatuses.Add(false);
        /// prot.ReadCoilStatus(1, 0, ref listOfCoilStatuses);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadCoilStatus(Byte rtuAddress, UInt16 startAddress, ref List<bool> statusesValues)
        {
            if(statusesValues.Count <=0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            bool[] tempArray = statusesValues.ToArray();
            ModbusErrorCode code = ReadStatuses(1, rtuAddress, startAddress, ref tempArray);
            statusesValues = tempArray.ToList();
            return code;
        }
        /// <summary>
        /// Reads statuses of inputs into array of boolean values
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>
        /// <param name="statusesValues">array of returned values</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads statuses of inputs from modbus slave 1 starting at address 0 and converts into boolean array of size 4.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// bool[] arrayOfInputStatuses = { false, false, false, false};
        /// prot.ReadInputStatus(1, 0, ref arrayOfCoilStatuses);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadInputStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues)
        {
            return ReadStatuses(2, rtuAddress, startAddress, ref statusesValues);
        }
        /// <summary>
        /// Reads statuses of inputs into list of boolean values
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>
        /// <param name="statusesValues">list of returned values</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads statuses of inputs from modbus slave 1 starting at address 0 and converts into boolean list of size 3.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// List&lt;bool&gt; listOfCoilStatuses = new List&lt;bool&gt;();
        /// listOfCoilStatuses.Add(false);
        /// listOfCoilStatuses.Add(false);
        /// listOfCoilStatuses.Add(false);
        /// prot.ReadInputStatus(1, 0, ref listOfCoilStatuses);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadInputStatus(Byte rtuAddress, UInt16 startAddress, ref List<bool> statusesValues)
        {
            if (statusesValues.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            bool[] tempArray = statusesValues.ToArray();
            ModbusErrorCode code = ReadStatuses(2, rtuAddress, startAddress, ref tempArray);            
            statusesValues = tempArray.ToList();
            return code;
        }
        /// <summary>
        /// Reads 16bit holding registers from Modbus device and convert them to array of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">array of returned values</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>       
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads 16 bit holding registers from modbus slave 1 starting at address 0 and converts them to array of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// object[] arrayMixedTypeRegisters = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
        /// prot.ReadHoldingRegisters(1, 0, ref arrayMixedTypeRegisters);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder=false)
        {
            return ReadRegisters(3, rtuAddress, startAddress, ref registerValues, 0, (UInt16)registerValues.Length, bigEndianOrder);
        }
        /// <summary>
        /// Reads 16bit holding registers from Modbus device and convert them into list of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">list of returned values (permitted only numeric types)</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>       
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads 16 bit holding registers from modbus slave 1 starting at address 0 and converts them to list of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// List&lt;object&gt; listMixedTypeRegisters = new List&lt;object&gt;();
        /// listMixedTypeRegisters.Add((Byte)0);
        /// listMixedTypeRegisters.Add((SByte)0);
        /// listMixedTypeRegisters.Add((Int16)0);
        /// listMixedTypeRegisters.Add((UInt16)0);
        /// listMixedTypeRegisters.Add((Single)0.0);        
        /// prot.ReadHoldingRegisters(1, 0, ref listMixedTypeRegisters);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, bool bigEndianOrder = false)
        {
            if (registerValues.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            object[] tempArray = registerValues.ToArray();
            ModbusErrorCode code = ReadRegisters(3, rtuAddress, startAddress, ref tempArray, 0, (UInt16)tempArray.Length, bigEndianOrder);
            registerValues = tempArray.ToList(); 
            return code;
        }
        /// <summary>
        /// Reads 16bit input registers from Modbus device and convert them to array of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">array of returned values (permitted only numeric types)</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>        
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads registers from modbus slave 1 starting at address 0 and converts them to array of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// object[] arrayMixedTypeRegisters = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
        /// prot.ReadInputRegisters(1, 0, ref arrayMixedTypeRegisters,true);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder=false)
        {
            return ReadRegisters(4, rtuAddress, startAddress, ref registerValues, 0, (UInt16)registerValues.Length, bigEndianOrder);
        }
        /// <summary>
        /// Reads 16bit input registers from Modbus device and convert them into list of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">list of returned values (permitted only numeric types)</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>       
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads 16 bit input registers from modbus slave 1 starting at address 0 and converts them to list of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// List&lt;object&gt; listMixedTypeRegisters = new List&lt;object&gt;();
        /// listMixedTypeRegisters.Add((Byte)0);
        /// listMixedTypeRegisters.Add((SByte)0);
        /// listMixedTypeRegisters.Add((Int16)0);
        /// listMixedTypeRegisters.Add((UInt16)0);
        /// listMixedTypeRegisters.Add((Single)0.0);        
        /// prot.ReadInputRegisters(1, 0, ref listMixedTypeRegisters);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues,bool bigEndianOrder = false)
        {
            if (registerValues.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            object[] tempArray = registerValues.ToArray();
            ModbusErrorCode code = ReadRegisters(4, rtuAddress, startAddress, ref tempArray, 0, (UInt16)tempArray.Length, bigEndianOrder);
            registerValues = tempArray.ToList();            
            return code;
        }
      
        /// <summary>
        /// Reads 16bit holding registers from Modbus device and convert them to array of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">array of returned values</param>
        /// <param name="startIndex">start position in array to which objects will be read</param>
        /// <param name="objectsCount">count of objects that will be read</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>       
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads 16 bit holding registers from modbus slave 1 starting at address 0 and converts them to array of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// object[] arrayMixedTypeRegisters = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
        /// prot.ReadHoldingRegisters(1, 0, ref arrayMixedTypeRegisters);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            return ReadRegisters(3, rtuAddress, startAddress, ref registerValues, startIndex, objectsCount, bigEndianOrder);
        }
        /// <summary>
        /// Reads 16bit holding registers from Modbus device and convert them into list of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">list of returned values (permitted only numeric types)</param>
        /// <param name="startIndex">start position in list to which objects will be read</param>
        /// <param name="objectsCount">count of objects that will be read</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>       
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads 16 bit holding registers from modbus slave 1 starting at address 0 and converts them to list of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// List&lt;object&gt; listMixedTypeRegisters = new List&lt;object&gt;();
        /// listMixedTypeRegisters.Add((Byte)0);
        /// listMixedTypeRegisters.Add((SByte)0);
        /// listMixedTypeRegisters.Add((Int16)0);
        /// listMixedTypeRegisters.Add((UInt16)0);
        /// listMixedTypeRegisters.Add((Single)0.0);        
        /// prot.ReadHoldingRegisters(1, 0, ref listMixedTypeRegisters);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            if (registerValues.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            object[] tempArray = registerValues.ToArray();
            ModbusErrorCode code = ReadRegisters(3, rtuAddress, startAddress, ref tempArray, startIndex, objectsCount, bigEndianOrder);
            registerValues = tempArray.ToList();
            return code;
        }
        /// <summary>
        /// Reads 16bit input registers from Modbus device and convert them to array of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">array of returned values (permitted only numeric types)</param>
        /// <param name="startIndex">start position in array to which objects will be read</param>
        /// <param name="objectsCount">count of objects that will be read</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>        
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads registers from modbus slave 1 starting at address 0 and converts them to array of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// object[] arrayMixedTypeRegisters = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
        /// prot.ReadInputRegisters(1, 0, ref arrayMixedTypeRegisters,true);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            return ReadRegisters(4, rtuAddress, startAddress, ref registerValues, startIndex, objectsCount, bigEndianOrder);
        }
        /// <summary>
        /// Reads 16bit input registers from Modbus device and convert them into list of values of numeric types
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">list of returned values (permitted only numeric types)</param>
        /// <param name="startIndex">start position in list to which objects will be read</param>
        /// <param name="objectsCount">count of objects that will be read</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>       
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Reads 16 bit input registers from modbus slave 1 starting at address 0 and converts them to list of objects of mixed type.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// List&lt;object&gt; listMixedTypeRegisters = new List&lt;object&gt;();
        /// listMixedTypeRegisters.Add((Byte)0);
        /// listMixedTypeRegisters.Add((SByte)0);
        /// listMixedTypeRegisters.Add((Int16)0);
        /// listMixedTypeRegisters.Add((UInt16)0);
        /// listMixedTypeRegisters.Add((Single)0.0);        
        /// prot.ReadInputRegisters(1, 0, ref listMixedTypeRegisters);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            if (registerValues.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            object[] tempArray = registerValues.ToArray();
            ModbusErrorCode code = ReadRegisters(4, rtuAddress, startAddress, ref tempArray, startIndex, objectsCount, bigEndianOrder);
            registerValues = tempArray.ToList();
            return code;
        }       
        /// <summary>
        /// Sets single output coil to on/off state
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="forceAddress">address of coil which will be set (0..65535)</param>
        /// <param name="setOn">value to set to: on - true, off - false</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Sets coil with address 0 of modbus slave 1 to "ON" state.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);        
        /// prot.ForceSingleCoil(1, 0, true);
        /// prot.Disconnect();
        /// </code>
        /// </example>       
        public ModbusErrorCode ForceSingleCoil(Byte rtuAddress, UInt16 forceAddress, bool setOn)
        {
            return ForceSingle(5, rtuAddress, forceAddress, setOn);
        }
        /// <summary>
        /// Presets single 16 bit modbus register
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="forceAddress">address of register which will be set (0..65535)</param>
        /// <param name="setValue">value to set (can be one of next types: UInt16, Int16, Byte[2]</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Sets register with address 0 of modbus slave 1 to value of -123 (Int16 type).</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);        
        /// ModbusErrorCode code = prot.PresetSingleRegister(1, 0, (Int16)(-123));         
        /// prot.Disconnect();
        /// </code>
        /// </example>
        ///                 
        public ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, object setValue)
        {
            return ForceSingle(6, rtuAddress, forceAddress, setValue);
        }
        /// <summary>
        /// Sets a number of output coils to on/off states
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="forceAddress">start address of coils which will be set (0..65535)</param>
        /// <param name="values">array of boolean values to set (on - true, off - false)</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Sets 4 coils starting at address 0 of modbus slave 1 to "ON" state.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);  
        /// bool[] arrVals = new[] {true, true, true, true};
        /// ModbusErrorCode code = prot.ForceMultipleCoils(1, 0, arrVals);              
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ForceMultipleCoils(Byte rtuAddress, UInt16 forceAddress, bool[] values)
        {
            return ForceMultiple(15, rtuAddress, forceAddress, Array.ConvertAll(values, b => (object)b), 0, (UInt16)values.Length);
        }
        /// <summary>
        /// Sets a number of output coils to on/off states
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="forceAddress">start address of coils which will be set (0..65535)</param>
        /// <param name="values">list of boolean values to set (on - true, off - false)</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Sets 4 coils starting at address 0 of modbus slave 1 to "ON" state.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);  
        /// List&lt;bool&gt; lstVals = new List&lt;bool&gt; {true, true, true, true};
        /// ModbusErrorCode code = prot.ForceMultipleCoils(1, 0, lstVals);              
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode ForceMultipleCoils(Byte rtuAddress, UInt16 forceAddress, List<bool> values)
        {
            if (values.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            bool[] tempArray = values.ToArray();
            return ForceMultiple(15, rtuAddress, forceAddress, Array.ConvertAll(tempArray, b => (object)b), 0, (UInt16)tempArray.Length);                        
        }
        /// <summary>
        /// Presets a number of 16 bit modbus registers
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="forceAddress">address of register which will be set (0..65535)</param>
        /// <param name="values">array of values to set (permitted only numeric types)</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Sets registers of modbus slave 1 starting at address 0 to sequence of values from array of objects of various numeric types.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// object[] arrayValues = { Byte.MaxValue, UInt16.MinValue, SByte.MinValue, Int16.MinValue, UInt32.MaxValue, Int32.MinValue, Single.MaxValue, UInt64.MaxValue, Int64.MinValue, Double.MaxValue, Decimal.MaxValue };               
        /// ModbusErrorCode code = prot.PresetMultipleRegisters(1, 0, arrayValues);               
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, object[] values, bool bigEndianOrder = false)
        {
            //it was Array.ConvertAll(values, b => (object)b)
            return ForceMultiple(16, rtuAddress, forceAddress, values, 0, (UInt16)values.Length, bigEndianOrder);
        }
        /// <summary>
        /// Presets a number of 16 bit modbus registers
        /// </summary>
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="forceAddress">address of register which will be set (0..65535)</param>
        /// <param name="values">list of values to set (permitted only numeric types)</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>
        /// <returns>ModbusErrorCode.CodeOk on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        /// <example>
        /// <para>Sets registers of modbus slave 1 starting at address 0 to sequence of values from list of objects of various numeric types.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// List&lt;object&gt; lstRegs = new List&lt;object&gt; { Byte.MaxValue, UInt16.MinValue, SByte.MinValue, Int16.MinValue, UInt32.MaxValue, Int32.MinValue, Single.MaxValue, UInt64.MaxValue, Int64.MinValue, Double.MaxValue, Decimal.MaxValue };                
        /// code = prot.PresetMultipleRegisters(1, 0, lstRegs);
        /// prot.Disconnect();
        /// </code>
        /// </example>
        public ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, List<object> values,bool bigEndianOrder = false)
        {
            if (values.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            object[] tempArray = values.ToArray();
            return ForceMultiple(16, rtuAddress, forceAddress, tempArray, 0, (UInt16)tempArray.Length, bigEndianOrder);            
        }

        public ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, object[] values,
            UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            return ForceMultiple(16, rtuAddress, forceAddress, values, startIndex, objectsCount, bigEndianOrder);
        }

        public ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, List<object> values,
            UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            if (values.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            object[] tempArray = values.ToArray();
            return ForceMultiple(16, rtuAddress, forceAddress, tempArray, startIndex, objectsCount, bigEndianOrder);      
        }

        /// <summary>
        /// Validates recieved packet
        /// </summary>
        /// <param name="packetRecieved">Packet to validate</param>
        /// <param name="slaveAddress">Slave address which answer we expect to recieve</param>
        /// <param name="functionCode">Modbus function code which we expect to recieve</param>
        /// <param name="expectedPacketLength">Expected length of the recieved packet</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>ModbusErrorCode.CodeOk on success, error code otherwise</returns>
        protected ModbusErrorCode CheckPacket(Byte[] packetRecieved, Byte slaveAddress, Byte functionCode, Int32 expectedPacketLength)
        {
            if ((packetRecieved == null)||(packetRecieved.Length != expectedPacketLength))
                return ModbusErrorCode.CodeInvalidPacketLength;
            if (!Crc16.CheckCrc(packetRecieved)) 
                return ModbusErrorCode.CodeCrcError;
            if (slaveAddress != packetRecieved[0])
                return ModbusErrorCode.CodeInvalidSlaveAddress;
            if (functionCode != packetRecieved[1])
                return ModbusErrorCode.CodeInvalidFunction;
            
            return ModbusErrorCode.CodeOk;
        }               
        /// <summary>
        /// Creates Modbus packet
        /// </summary>
        /// <param name="slaveAddress">address of Modbus device (1..247)</param>
        /// <param name="functionCode">Modbus function code</param>
        /// <param name="startAddress">start address from which registers will be read/forced (0..65535)</param>
        /// <param name="quantity">count of registers/coils to be read (functionCode: 1,2,3,4) or forced (functionCode: 15,16)</param>
        /// <param name="forcedValues">array of converted values which are need to be written (functionCode: 5,6,15,16)</param>
        /// <returns>created packed</returns>                
        protected Byte[] MakePacket(Byte slaveAddress, Byte functionCode, UInt16 startAddress, UInt16 quantity, UInt16[] forcedValues = null, bool forceMultiple = false, bool forceCoils = false, bool bigEndianOrder = false)
        {
            List<Byte> sendPacketList = new List<Byte>();
            sendPacketList.Add(slaveAddress);
            sendPacketList.Add(functionCode);
            sendPacketList.Add(BitConverter.GetBytes(startAddress).ElementAt<Byte>(1));
            sendPacketList.Add(BitConverter.GetBytes(startAddress).ElementAt<Byte>(0));
            if (forcedValues == null)
            {
                sendPacketList.Add(BitConverter.GetBytes(quantity).ElementAt<Byte>(1));
                sendPacketList.Add(BitConverter.GetBytes(quantity).ElementAt<Byte>(0));
            }
            else
            {
                if (forceMultiple)
                {
                    sendPacketList.Add(BitConverter.GetBytes(quantity).ElementAt<Byte>(1));
                    sendPacketList.Add(BitConverter.GetBytes(quantity).ElementAt<Byte>(0));
                    if (forceCoils)
                        sendPacketList.Add((Byte)((quantity+7)/8));  
                    else
                        sendPacketList.Add((Byte)(forcedValues.Length * sizeof(UInt16)));    
                }
                int added = 0;
                foreach (var value in forcedValues)
                {
                    if (forceCoils && forceMultiple)
                    {
                        sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(0));
                        added++;
                        if (((quantity + 7)/8) > added)
                        {
                            sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(1));
                            added++;
                        }                                                    
                    }
                    else
                    {
                        if (bigEndianOrder)
                        {
                            sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(0));
                            sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(1));
                        }
                        else
                        {
                            sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(1));
                            sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(0));
                        }                        
                    }
                }
            }                                                  
            Byte[] sendPacket = sendPacketList.ToArray();
            Crc16.AddCrc(ref sendPacket);
            return sendPacket;
        }

        protected ModbusErrorCode ParseExceptionCode(Byte functionNumber, Byte rtuAddress, Byte[] recievedPacket)
        {
            if (recievedPacket.Length == 5)
            {
                if (rtuAddress != recievedPacket[0])
                    return ModbusErrorCode.CodeInvalidSlaveAddress;
                if (functionNumber + 0x80 != recievedPacket[1])
                    return ModbusErrorCode.CodeInvalidFunction;                
                if (Crc16.CheckCrc(recievedPacket) == false)
                    return ModbusErrorCode.CodeCrcError;
                return (ModbusErrorCode)(recievedPacket[2] + 0x8000);
            }            
            return ModbusErrorCode.CodeInvalidPacketLength;
        }

        protected ModbusErrorCode ReadRegisters(Byte functionNumber, Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {            
            if (!IsConnected)
                return ModbusErrorCode.CodeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.CodeInvalidSlaveAddress;
            if (startIndex + objectsCount > registerValues.Length)
                return ModbusErrorCode.CodeInvalidRequestedSize;

            try
            {
                UInt32 registersCount = 0, bytesCount = 0;
                for (UInt32 regVal = startIndex; regVal < startIndex + objectsCount; regVal++)
                {
                    if (GetTypeHelper.IsNumericType(registerValues[regVal].GetType()))
                        bytesCount += (UInt16)Marshal.SizeOf(registerValues[regVal]);
                    else
                        bytesCount += SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(registerValues[regVal]);
                }
                registersCount = (bytesCount + 1)/2;

                if ((registersCount > _readRegistersPerQueryCapacity) || (registersCount < 1) || (startAddress + registersCount > UInt16.MaxValue))
                    return ModbusErrorCode.CodeInvalidRequestedSize;

                Byte[] recievedPacket = null;

                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, startAddress, (UInt16) registersCount);

                ushort expectedRecievedPacketSize = (ushort) (5 + registersCount*2);
                if (TxRxMessage(sendPacket, ref recievedPacket, expectedRecievedPacketSize))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1],
                        expectedRecievedPacketSize);
                    if (errorCode == ModbusErrorCode.CodeOk)
                    {
                        Byte[] packetData = new Byte[recievedPacket.Length - 5];
                        Array.Copy(recievedPacket, 3, packetData, 0, packetData.Length);
                        try
                        {         
                            ModbusDataMappingHelper.SwapBytesInWordsInByteArray(ref packetData);
                            ModbusDataMappingHelper.ProcessAnalogData(packetData, ref registerValues, startIndex, objectsCount, bigEndianOrder);
                        }
                        catch (Exception ex)
                        {
                            if (_logExceptionModbusRtu != null)
                                _logExceptionModbusRtu(ex);
                            return ModbusErrorCode.CodeExceptionInCodeOccured;
                        }
                        //TODO here we need to log the StatusString
                        return ModbusErrorCode.CodeOk;
                    }
                    //TODO here we need to log the StatusString
                    return errorCode;
                }
                
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.CodeTimeout;

                if (StatusString.Contains("Error: Invalid response length"))                
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                
                return ModbusErrorCode.CodeErrorSendingPacket;
            }            
            catch (Exception ex)
            {
                if (_logExceptionModbusRtu != null)
                    _logExceptionModbusRtu(ex);
                return ModbusErrorCode.CodeExceptionInCodeOccured;
            }
        }

        protected ModbusErrorCode ReadStatuses(Byte functionNumber, Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues)
        {
            if (!IsConnected)
                return ModbusErrorCode.CodeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.CodeInvalidSlaveAddress;

            if ((statusesValues.Length > 2000) || (statusesValues.Length < 1) || (startAddress + statusesValues.Length > UInt16.MaxValue))
                return ModbusErrorCode.CodeInvalidRequestedSize;

            Byte[] recievedPacket = null;

            ushort expectedRecievedPacketSize = (ushort)(5 + (statusesValues.Length + 7) / 8);
            
            try
            {
                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, startAddress, (UInt16)statusesValues.Length);                

                if (TxRxMessage(sendPacket, ref recievedPacket, expectedRecievedPacketSize))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1], expectedRecievedPacketSize);
                    if (errorCode == ModbusErrorCode.CodeOk)
                    {
                        Byte[] packetData = new Byte[recievedPacket.Length - 5];
                        Array.Copy(recievedPacket, 3, packetData, 0, packetData.Length);

                        try
                        {
                            ModbusDataMappingHelper.ProcessDiscreetData(packetData, ref statusesValues);                            
                        }
                        catch (Exception ex)
                        {
                            if (_logExceptionModbusRtu != null)
                                _logExceptionModbusRtu(ex);
                            return ModbusErrorCode.CodeExceptionInCodeOccured;
                        }
                        //TODO here we need to log the StatusString
                        return ModbusErrorCode.CodeOk;
                    }
                    
                    //TODO here we need to log the StatusString
                    return errorCode;
                }                
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.CodeTimeout;
                if (StatusString.Contains("Error: Invalid response length"))
                {
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                }
                return ModbusErrorCode.CodeErrorSendingPacket;               
            }
            catch (Exception ex)
            {
                if (_logExceptionModbusRtu != null)
                    _logExceptionModbusRtu(ex);
                return ModbusErrorCode.CodeExceptionInCodeOccured;
            }            
        }

        protected ModbusErrorCode ForceSingle(Byte functionNumber, Byte rtuAddress, UInt16 forceAddress, object value)
        {
            if (!IsConnected)
                return ModbusErrorCode.CodeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.CodeInvalidSlaveAddress;
            if ((value.GetType() != typeof(Int16)) && (value.GetType() != typeof(UInt16)) && (value.GetType() != typeof(Byte[])) && (value.GetType() != typeof(bool)))
                return ModbusErrorCode.CodeInvalidInputArgument;

            Byte[] recievedPacket = null;

            UInt16[] forcedValue = new ushort[1] { 0x0000 };
            if (value.GetType() == typeof(bool))
            {
                if ((bool)value)
                    forcedValue[0] = 0xFF00;
            }
            else if (value.GetType() == typeof(Int16))            
                forcedValue[0] = BitConverter.ToUInt16(BitConverter.GetBytes((Int16)value), 0);            
            else if (value.GetType() == typeof(Byte[]))                            
                forcedValue[0] = BitConverter.ToUInt16((Byte[])value,0);
            else           
                forcedValue[0] = (UInt16)value;           
                
          
            try
            {                                
                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, forceAddress, 1, forcedValue);
                
                if (TxRxMessage(sendPacket, ref recievedPacket, 8))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1], 8);
                    if (errorCode == ModbusErrorCode.CodeOk)
                    {
                        if (recievedPacket.SequenceEqual(sendPacket))
                        {
                            //TODO here we need to log the StatusString
                            return ModbusErrorCode.CodeOk;
                        }
                        return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                    }                   
                    //TODO here we need to log the StatusString
                    return errorCode;                    
                }               
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.CodeTimeout;
                if (StatusString.Contains("Error: Invalid response length"))                
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                
                return ModbusErrorCode.CodeErrorSendingPacket;                
            }
            catch (Exception ex)
            {
                if (_logExceptionModbusRtu != null)
                    _logExceptionModbusRtu(ex);
                return ModbusErrorCode.CodeExceptionInCodeOccured;        
            }            
        }

        protected ModbusErrorCode ForceMultiple(Byte functionNumber, Byte rtuAddress, UInt16 forceAddress, object[] values, UInt32 startIndex, UInt32 objectsCount, bool bigEndianOrder = false)
        {
            if (!IsConnected)
                return ModbusErrorCode.CodeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.CodeInvalidSlaveAddress;
            if (startIndex + objectsCount > values.Length)
                return ModbusErrorCode.CodeInvalidRequestedSize;

            ushort[] forcedValues;
            Type firstElementType;

            for (Int32 i = (Int32)(objectsCount + startIndex - 1); i >= startIndex; i--)
            {
                if (
                    SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(values[i],
                        ModbusRegisterAccessType.AccessReadWrite) == 0)
                {
                    values = values.Where((val, idx) => idx != i).ToArray();
                    objectsCount--;
                }
            }            
           
            try
            {
                ModbusDataMappingHelper.ConvertObjectsVaulesToRegisters(values, startIndex, objectsCount,
                    bigEndianOrder, out forcedValues, out firstElementType);
            }
            catch (Exception)
            {
                return ModbusErrorCode.CodeInvalidInputArgument;
            }

            if ((forcedValues.Length > _writeRegistersPerQueryCapacity) || (forcedValues.Length < 1) ||
                (forceAddress + forcedValues.Length > UInt16.MaxValue))
                return ModbusErrorCode.CodeInvalidRequestedSize;   

            Byte[] recievedPacket = null;           

            try
            {
                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, forceAddress, (firstElementType == typeof(bool)) ? (UInt16)(startIndex + objectsCount) : (UInt16)forcedValues.Length, forcedValues, true, firstElementType == typeof(bool), bigEndianOrder);
                
                if (TxRxMessage(sendPacket, ref recievedPacket, 8))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1], 8);
                    if (errorCode == ModbusErrorCode.CodeOk)
                    {
                        Byte[] tmp = BitConverter.GetBytes((firstElementType == typeof(bool)) ? (UInt16)(startIndex + objectsCount) : (UInt16)forcedValues.Length);
                        Array.Reverse(tmp); 
                        if (BitConverter.ToUInt16(recievedPacket, 4) != BitConverter.ToUInt16(tmp, 0))
                            return ModbusErrorCode.CodeInvalidResponse;
                        tmp = BitConverter.GetBytes(forceAddress);
                        Array.Reverse(tmp);
                        if (BitConverter.ToUInt16(recievedPacket, 2) != BitConverter.ToUInt16(tmp, 0))
                            return ModbusErrorCode.CodeInvalidResponse;                                                      
                        //TODO here we need to log the StatusString
                        return ModbusErrorCode.CodeOk;
                    }
                    
                    //TODO here we need to log the StatusString
                    return errorCode;                    
                }
                
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.CodeTimeout;
                    
                if (StatusString.Contains("Error: Invalid response length"))
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                
                return ModbusErrorCode.CodeErrorSendingPacket;                
            }
            catch (Exception ex)
            {
                if (_logExceptionModbusRtu != null)
                    _logExceptionModbusRtu(ex);
                return ModbusErrorCode.CodeExceptionInCodeOccured;
            }            
        }        

        private Byte _readRegistersPerQueryCapacity;
        private Byte _writeRegistersPerQueryCapacity;

        public Byte ReadRegistersPerQueryCapacity
        {
            get { return _readRegistersPerQueryCapacity; }
            set
            {
                if (value > 125) _readRegistersPerQueryCapacity = 125;
                else if (value == 0) _readRegistersPerQueryCapacity = 1;
                else _readRegistersPerQueryCapacity = value;
            }
        }
        public Byte WriteRegistersPerQueryCapacity
        {
            get { return _writeRegistersPerQueryCapacity; }
            set
            {
                if (value > 125) _writeRegistersPerQueryCapacity = 125;
                else if (value == 0) _readRegistersPerQueryCapacity = 1;
                else _writeRegistersPerQueryCapacity = value;
            }
        }
    }   
}
