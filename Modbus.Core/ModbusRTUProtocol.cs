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
        public ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
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
        public ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
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
        public ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
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
        public ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref List<object> registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
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
            UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
        {
            return ForceMultiple(16, rtuAddress, forceAddress, values, startIndex, objectsCount, bigEndianOrder);
        }

        public ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, List<object> values,
            UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
        {
            if (values.Count <= 0)
                return ModbusErrorCode.CodeInvalidRequestedSize;
            object[] tempArray = values.ToArray();
            return ForceMultiple(16, rtuAddress, forceAddress, tempArray, startIndex, objectsCount, bigEndianOrder);      
        }

        /// <summary>
        /// Adds two bytes of CRC16 to array representing Modbus protocol data packet passed in argument
        /// </summary>
        /// <param name="packet">data packet to which we need to add CRC16</param>
        ///  <exception cref="System.ArgumentNullException">Thrown if packet is null</exception>
        ///  <exception cref="System.ArgumentOutOfRangeException">Thrown if argument length &lt; 2 or &gt; 254</exception>
        protected void AddCrc(ref Byte[] packet)
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
                        crc ^= 0xA001;
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
        /// <returns>true on success, otherwise false</returns>            
        protected bool CheckCrc(Byte[] packet)
        {
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
            if (!CheckCrc(packetRecieved)) 
                return ModbusErrorCode.CodeCrcError;
            if (slaveAddress != packetRecieved[0])
                return ModbusErrorCode.CodeInvalidSlaveAddress;
            if (functionCode != packetRecieved[1])
                return ModbusErrorCode.CodeInvalidFunction;
            
            return ModbusErrorCode.CodeOk;
        }
        /// <summary>
        /// Fills array of objects with values from array of raw data extracted from modbus packet
        /// </summary>
        /// <param name="rawPacketData">raw data extracted from modbus packet</param>
        /// <param name="outputValues">array of objects to fill</param>
        /// <param name="startIndex">start position in array to which objects will be filled</param>
        /// <param name="objectsCount">count of objects that will be filled</param>
        /// <param name="bigEndianOrder">if true modbus registers are processed to 32bit(or higher) values in big endian order: first register - high 16bit, second - low 16bit</param>
        /// <returns>true on success, false otherwise</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.NullReferenceException"></exception>
        /// <remarks>outputValues can contain numeric types and classes or structs with public numeric properties</remarks>
        protected void ProcessAnalogData(Byte[] rawPacketData, ref object[] outputValues,UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
        {
            if(rawPacketData == null)
                throw new ArgumentNullException();
            if (startIndex + objectsCount > outputValues.Length)
                throw new ArgumentOutOfRangeException();

            UInt32 totalLengthInBytesOfRequestedData = 0;
            int currentIndexInPacketData = 0;
           
            for (int val = startIndex; val < startIndex + objectsCount; val++)
            {
                if (outputValues[val] == null)
                    throw new NullReferenceException();

                if (outputValues[val].GetType().IsValueType)//here we will process simple (only numeric) types
                {
                    if (GetTypeHelper.IsNumericType(outputValues[val].GetType()))
                    {
                        totalLengthInBytesOfRequestedData += (UInt32)Marshal.SizeOf(outputValues[val]);
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            throw new ArgumentOutOfRangeException();                        
                        ModbusDataMappingHelper.ExtractValueFromArrayByType(rawPacketData,
                            ref currentIndexInPacketData, ref outputValues[val], bigEndianOrder);
                    }
                    else
                        throw new ArgumentException("Neither numeric nor class value in outputValues array");
                }
                else//here we will process properties (only numeric) from complex (class) types 
                {
                    Type[] arrayOfOutputTypes = ModbusDataMappingHelper.GetObjectModbusPropertiesTypeArray(outputValues[val]);
                    totalLengthInBytesOfRequestedData += SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(outputValues[val]);
                    if (totalLengthInBytesOfRequestedData > 0)
                    {
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            throw new ArgumentOutOfRangeException();
                        object[] arrayOfValuesForObject =
                            ModbusDataMappingHelper.CreateValuesArrayFromTypesArray(arrayOfOutputTypes);

                        for (int i = 0; i < arrayOfValuesForObject.Length; i++)
                        {
                            ModbusDataMappingHelper.ExtractValueFromArrayByType(rawPacketData,
                                ref currentIndexInPacketData, ref arrayOfValuesForObject[i], bigEndianOrder);
                        }
                        ModbusDataMappingHelper.SetObjectPropertiesValuesFromArray(
                            ref outputValues[val], arrayOfValuesForObject);                                        
                    }                    
                }               
            }                    
        }
        /// <summary>
        /// Fills array of bool values from modbus packet
        /// </summary>
        /// <param name="rawPacketData">raw data extracted from modbus packet</param>
        /// <param name="outputValues">array of boolean values to fill</param>        
        /// <returns>true on success, false otherwise</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>        
        /// <remarks>outputValues can contain numeric types and classes or structs with public numeric properties</remarks>
        protected void ProcessDiscreetData(Byte[] rawPacketData, ref bool[] outputValues)
        {
            if (rawPacketData == null)
                throw new ArgumentNullException();
            
            if (outputValues.Length > rawPacketData.Length*8)
                throw new ArgumentOutOfRangeException();
            
            int deltaResize = rawPacketData.Length*8 - outputValues.Length;

            Array.Resize<bool>(ref outputValues, rawPacketData.Length * 8);
            BitArray bitsOfRawPacketData = new BitArray(rawPacketData);
            bitsOfRawPacketData.CopyTo(outputValues,0);

            Array.Resize<bool>(ref outputValues, outputValues.Length - deltaResize);                        
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
            AddCrc(ref sendPacket);
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
                if (CheckCrc(recievedPacket) == false)
                    return ModbusErrorCode.CodeCrcError;
                return (ModbusErrorCode)(recievedPacket[2] + 0x8000);
            }            
            return ModbusErrorCode.CodeInvalidPacketLength;
        }

        protected ModbusErrorCode ReadRegisters(Byte functionNumber, Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
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
                for (int regVal = startIndex; regVal < startIndex + objectsCount; regVal++)
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
                            ProcessAnalogData(packetData, ref registerValues,startIndex, objectsCount, bigEndianOrder);
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
                            ProcessDiscreetData(packetData, ref statusesValues);                            
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

        protected ModbusErrorCode ForceMultiple(Byte functionNumber, Byte rtuAddress, UInt16 forceAddress, object[] values, UInt16 startIndex, UInt16 objectsCount, bool bigEndianOrder = false)
        {
            if (!IsConnected)
                return ModbusErrorCode.CodeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.CodeInvalidSlaveAddress;
            if (startIndex + objectsCount > values.Length)
                return ModbusErrorCode.CodeInvalidRequestedSize;
                        
            //array of output 16bit values
            UInt16[] forcedValues = null;

            //only for determine if we need to preset multiple coils
            Type firstElementType = values[0].GetType();
            bool[] boolValues  = null;
            //if firstElementType is boolean then all elements of array are considered to be boolean type also
            if (firstElementType == typeof (bool))
            {
                if (startIndex + objectsCount > 2000)//can force maximum 2000 coils
                    return ModbusErrorCode.CodeInvalidInputArgument;
                Array.Resize(ref forcedValues, (startIndex + objectsCount + 15) / 16);
                Array.Resize(ref boolValues, startIndex + objectsCount);
            }
            Byte[] tempArrayOfBytes = null;
            UInt32 totalLengthInBytesOfValuesToBeSet = 0;
            int i = 0;
            //foreach (var value in values)
            for (int regVal = startIndex; regVal < startIndex + objectsCount; regVal++)
            {
                //if arrayType is bool - all elements must be bool
                if ((firstElementType == typeof(bool)) && (firstElementType != values[regVal].GetType()))
                    return ModbusErrorCode.CodeInvalidInputArgument;
                if (firstElementType == typeof(bool))
                    boolValues[i] = (bool)values[regVal];

                //if another type 
                if (values[regVal].GetType().IsValueType) //here we will process simple (only numeric) types
                {
                    if (GetTypeHelper.IsNumericType(values[regVal].GetType()))
                    {
                        totalLengthInBytesOfValuesToBeSet += (UInt32)Marshal.SizeOf(values[regVal]);
                        if (totalLengthInBytesOfValuesToBeSet > _writeRegistersPerQueryCapacity * 2) //maximum _writeRegistersPerQueryCapacity*2 bytes can be processed
                            return ModbusErrorCode.CodeInvalidInputArgument;
                        ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref tempArrayOfBytes, values[regVal], bigEndianOrder);
                    }
                    else if (values[regVal].GetType() != typeof(bool))
                        return ModbusErrorCode.CodeInvalidInputArgument;                                            
                }
                else//here we will process properties (only numeric) from complex (class) types 
                {
                    totalLengthInBytesOfValuesToBeSet += SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(values[regVal]);
                    if (totalLengthInBytesOfValuesToBeSet > _writeRegistersPerQueryCapacity * 2)//maximum _writeRegistersPerQueryCapacity*2 bytes can be processed
                        return ModbusErrorCode.CodeInvalidInputArgument;
                    if (totalLengthInBytesOfValuesToBeSet > 0)
                    {
                        object[] arrayOfObjectPropsValues = null;
                        try
                        {
                            //arrayOfObjectPropsTypes = ModbusDataMappingHelper.GetObjectModbusPropertiesTypeArray(value);
                            arrayOfObjectPropsValues = ModbusDataMappingHelper.GetObjectModbusPropertiesValuesArray(values[regVal]);
                        }
                        catch (Exception ex)
                        {
                            if (_logExceptionModbusRtu != null)
                                _logExceptionModbusRtu(ex);
                            return ModbusErrorCode.CodeInvalidInputArgument;
                        }

                        for (int k = 0; k < arrayOfObjectPropsValues.Length; k++)
                        {
                            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref tempArrayOfBytes, arrayOfObjectPropsValues[k],
                                bigEndianOrder);
                        }                       
                    }                    
                }               
                i++;
            }

            if (firstElementType == typeof(bool))
            {
                Byte[] temp = new Byte[forcedValues.Length*2];
                new BitArray(boolValues).CopyTo(temp, 0);                
                Buffer.BlockCopy(temp, 0, forcedValues, 0, temp.Length);
            }
            else
            {
                Array.Resize(ref forcedValues, (tempArrayOfBytes.Length + 1) / 2);
                Buffer.BlockCopy(tempArrayOfBytes, 0, forcedValues, 0, tempArrayOfBytes.Length);                
            }


            if ((forcedValues.Length > _writeRegistersPerQueryCapacity) || (forcedValues.Length < 1) || (forceAddress + forcedValues.Length > UInt16.MaxValue))
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
