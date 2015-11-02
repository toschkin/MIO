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
    public class ModbusRTUProtocol : RawSerialProtocol, IModbus
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ModbusRTUProtocol()            
        {

        }

        /// <summary>
        /// Exception saving
        /// </summary>
        private SaveException _logExceptionModbusRTU;
        /// <summary>
        /// Adds logger to list of exceptions saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void SaveException(Exception message);</param>
        public void AddExceptionsLogger(SaveException logger)
        {
            _logExceptionModbusRTU += logger;
            LogExceptionRsp += logger;
        }
        /// <summary>
        /// Removes logger from current list of exceptions saving handlers
        /// </summary>
        /// <param name="logger">delegate of form: void SaveException(Exception message);</param>
        public void RemoveExceptionsLogger(SaveException logger)
        {
            _logExceptionModbusRTU += logger;
            LogExceptionRsp -= logger;
        }

        public ModbusErrorCode ReadCoilStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues)
        {
            return ReadStatuses(1, rtuAddress, startAddress, ref statusesValues);
        }

        public ModbusErrorCode ReadInputStatus(Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues)
        {
            return ReadStatuses(2, rtuAddress, startAddress, ref statusesValues);
        }
       
        /// <summary>
        /// Reads 16bit holding registers from Modbus device and convert them to array of values of appropriate type
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">array of returned values</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>
        /// <example>
        /// <para>Reads registers from modbus slave 1 starting at address 0 and converts them to array of objects of mixed type in .</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// object[] arrayMixedTypeRegisters = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
        /// prot.ReadHoldingRegisters&lt;UInt32&gt;(1, 0, ref arrayMixedTypeRegisters,true);
        /// </code>
        /// </example>
        /// <returns> ModbusErrorCode.codeOK on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        public ModbusErrorCode ReadHoldingRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder=false)
        {
            return ReadRegisters(3, rtuAddress, startAddress, ref registerValues, bigEndianOrder);
        }
         /// <summary>
        /// Reads 16bit input registers from Modbus device and convert them to array of values of appropriate type
        /// </summary>        
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>      
        /// <param name="registerValues">array of returned values</param>
        /// <param name="bigEndianOrder">reorders registers in 32bit (or higher) values</param>
        /// <example>
        /// <para>Reads registers from modbus slave 1 starting at address 0 and converts them to array of objects of mixed type in .</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// object[] arrayMixedTypeRegisters = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
        /// prot.ReadInputRegisters&lt;UInt32&gt;(1, 0, ref arrayMixedTypeRegisters,true);
        /// </code>
        /// </example>
        /// <returns> ModbusErrorCode.codeOK on success (<see cref=Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        public ModbusErrorCode ReadInputRegisters(Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder=false)
        {
            return ReadRegisters(4, rtuAddress, startAddress, ref registerValues, bigEndianOrder);
        }

        public ModbusErrorCode ForceSingleCoil(Byte rtuAddress, UInt16 forceAddress, bool setOn)
        {
            return ForceSingle(5, rtuAddress, forceAddress, setOn);
        }

        public ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, UInt16 setValue)
        {
            return ForceSingle(6, rtuAddress, forceAddress, setValue);
        }

        public ModbusErrorCode PresetSingleRegister(Byte rtuAddress, UInt16 forceAddress, Int16 setValue)
        {
            return ForceSingle(6, rtuAddress, forceAddress, setValue);
        }

        public ModbusErrorCode ForceMultipleCoils(Byte rtuAddress, UInt16 forceAddress, bool[] values)
        {
            return ForceMultiple(15, rtuAddress, forceAddress, Array.ConvertAll(values, b => (object) b));
        }

        public ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, UInt16[] values)
        {
            return ForceMultiple(16, rtuAddress, forceAddress, Array.ConvertAll(values, b => (object)b));
        }

        public ModbusErrorCode PresetMultipleRegisters(Byte rtuAddress, UInt16 forceAddress, Int16[] values)
        {
            return ForceMultiple(16, rtuAddress, forceAddress, Array.ConvertAll(values, b => (object)b));
        }        

        /// <summary>
        /// Adds two bytes of CRC16 to array representing Modbus protocol data packet passed in argument
        /// </summary>
        /// <param name="packet">data packet to which we need to add CRC16</param>
        ///  <exception cref="System.ArgumentNullException">Thrown if packet is null</exception>
        ///  <exception cref="System.ArgumentOutOfRangeException">Thrown if argument length &lt; 2 or &gt; 254</exception>
        protected void AddCRC(ref Byte[] packet)
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
            List<byte> listCRC = new List<byte>();
            listCRC.AddRange(packet);
            listCRC.Add(BitConverter.GetBytes(crc).ElementAt<Byte>(0));
            listCRC.Add(BitConverter.GetBytes(crc).ElementAt<Byte>(1));
            Array.Resize(ref packet, listCRC.Count);
            listCRC.ToArray().CopyTo(packet, 0);                                             
        }
        /// <summary>
        /// Checks CRC16 of Modbus protocol data packet passed in argument
        /// </summary>
        /// <param name="packet"> packet, which CRC we need to verify</param>
        /// <returns>true on success, otherwise false</returns>            
        protected bool CheckCRC(Byte[] packet)
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
        /// <returns>ModbusErrorCode.codeOK on success, error code otherwise</returns>
        protected ModbusErrorCode CheckPacket(Byte[] packetRecieved, Byte slaveAddress, Byte functionCode, Int32 expectedPacketLength)
        {
            if ((packetRecieved == null)||(packetRecieved.Length != expectedPacketLength))
                return ModbusErrorCode.codeInvalidPacketLength;
            if (!CheckCRC(packetRecieved)) 
                return ModbusErrorCode.codeCRCError;
            if (slaveAddress != packetRecieved[0])
                return ModbusErrorCode.codeInvalidSlaveAddress;
            if (functionCode != packetRecieved[1])
                return ModbusErrorCode.codeInvalidFunction;
            
            return ModbusErrorCode.codeOK;
        }
        /// <summary>
        /// Fills array of objects with values from array of raw data extracted from modbus packet
        /// </summary>
        /// <param name="rawPacketData">raw data extracted from modbus packet</param>
        /// <param name="outputValues">array of objects to fill</param>
        /// <param name="bigEndianOrder">if true modbus registers are processed to 32bit(or higher) values in big endian order: first register - high 16bit, second - low 16bit</param>
        /// <returns>true on success, false otherwise</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.NullReferenceException"></exception>
        /// <remarks>outputValues can contain numeric types and classes or structs with public numeric properties</remarks>
        protected void ProcessAnalogData(Byte[] rawPacketData, ref object[] outputValues, bool bigEndianOrder = false)
        {
            if(rawPacketData == null)
                throw new ArgumentNullException();

            UInt32 totalLengthInBytesOfRequestedData = 0;
            int currentIndexInPacketData = 0;
            int currentIndexInOutputValues = 0;
            foreach (var value in outputValues)
            {
                if (value == null)
                    throw new NullReferenceException();

                if (value.GetType().IsValueType)//here we will process simple (only numeric) types
                {
                    if (GetTypeHelper.IsNumericType(value.GetType()))
                    {
                        totalLengthInBytesOfRequestedData += (UInt32)Marshal.SizeOf(value);
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            throw new ArgumentOutOfRangeException();                  
                        ModbusDataMappingHelper.ExtractValueFromArrayByType(rawPacketData,
                            ref currentIndexInPacketData, ref outputValues[currentIndexInOutputValues], bigEndianOrder);                        
                    }
                }
                else//here we will process properties (only numeric) from complex (class) types 
                {
                    
                    Type[] arrayOfOutputTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(value);
                    totalLengthInBytesOfRequestedData += SizeofHelper.SizeOfPublicProperties(value);
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
                        ref outputValues[currentIndexInOutputValues], arrayOfValuesForObject);                                     
                }
                currentIndexInOutputValues++;
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
            

            /*UInt32 totalLengthInBytesOfRequestedData = 0;
            int currentIndexInPacketData = 0;
            int currentIndexInOutputValues = 0;
            foreach (var value in outputValues)
            {                
                if (value.GetType().IsValueType)//here we will process simple (only numeric) types
                {
                    if (GetTypeHelper.IsNumericType(value.GetType()))
                    {
                        totalLengthInBytesOfRequestedData += (UInt32)Marshal.SizeOf(value);
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            throw new ArgumentOutOfRangeException();
                        ModbusDataMappingHelper.ExtractValueFromArrayByType(rawPacketData,
                            ref currentIndexInPacketData, ref outputValues[currentIndexInOutputValues], bigEndianOrder);
                    }
                }
                else//here we will process properties (only numeric) from complex (class) types 
                {

                    Type[] arrayOfOutputTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(value);
                    totalLengthInBytesOfRequestedData += SizeofHelper.SizeOfPublicProperties(value);
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
                        ref outputValues[currentIndexInOutputValues], arrayOfValuesForObject);
                }
                currentIndexInOutputValues++;
            }*/
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
        protected Byte[] MakePacket(Byte slaveAddress, Byte functionCode, UInt16 startAddress, UInt16 quantity, UInt16[] forcedValues = null, bool forceMultiple = false, bool forceCoils = false)
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
                        sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(1));
                        sendPacketList.Add(BitConverter.GetBytes(value).ElementAt<Byte>(0));
                    }
                }
            }                                                  
            Byte[] sendPacket = sendPacketList.ToArray();
            AddCRC(ref sendPacket);
            return sendPacket;
        }

        protected ModbusErrorCode ParseExceptionCode(Byte functionNumber, Byte rtuAddress, Byte[] recievedPacket)
        {
            if (recievedPacket.Length == 5)
            {
                if (rtuAddress != recievedPacket[0])
                    return ModbusErrorCode.codeInvalidSlaveAddress;
                if (functionNumber + 0x80 != recievedPacket[1])
                    return ModbusErrorCode.codeInvalidFunction;                
                if (CheckCRC(recievedPacket) == false)
                    return ModbusErrorCode.codeCRCError;
                return (ModbusErrorCode)(recievedPacket[2] + 0x8000);
            }            
            return ModbusErrorCode.codeInvalidPacketLength;
        }

        protected ModbusErrorCode ReadRegisters(Byte functionNumber, Byte rtuAddress, UInt16 startAddress, ref object[] registerValues, bool bigEndianOrder = false)
        {            
            if (!IsConnected)
                return ModbusErrorCode.codeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.codeInvalidSlaveAddress;

            try
            {
                UInt32 registersCount = 0;
                registersCount = (SizeofHelper.SizeOfPublicProperties(registerValues)%2) > 0
                    ? SizeofHelper.SizeOfPublicProperties(registerValues)/2
                    : (SizeofHelper.SizeOfPublicProperties(registerValues)/2) + 1;
                if ((registersCount > 125) || (registersCount < 1) || (startAddress + registersCount > UInt16.MaxValue))
                    return ModbusErrorCode.codeInvalidRequestedSize;

                Byte[] recievedPacket = null;

                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, startAddress, (UInt16) registersCount);

                ushort expectedRecievedPacketSize = (ushort) (5 + registersCount*2);
                if (TxRxMessage(sendPacket, ref recievedPacket, expectedRecievedPacketSize))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1],
                        expectedRecievedPacketSize);
                    if (errorCode == ModbusErrorCode.codeOK)
                    {
                        Byte[] packetData = new Byte[recievedPacket.Length - 5];
                        Array.Copy(recievedPacket, 3, packetData, 0, packetData.Length);
                        try
                        {
                            ProcessAnalogData(packetData, ref registerValues, bigEndianOrder);
                        }
                        catch (Exception ex)
                        {
                            if (_logExceptionModbusRTU != null)
                                _logExceptionModbusRTU(ex);
                            return ModbusErrorCode.codeExceptionInCodeOccured;
                        }
                        //TODO here we need to log the StatusString
                        return ModbusErrorCode.codeOK;
                    }
                    //TODO here we need to log the StatusString
                    return errorCode;
                }
                
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.codeTimeout;

                if (StatusString.Contains("Error: Invalid response length"))                
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                
                return ModbusErrorCode.codeErrorSendingPacket;
            }            
            catch (Exception ex)
            {
                if (_logExceptionModbusRTU != null)
                    _logExceptionModbusRTU(ex);
                return ModbusErrorCode.codeExceptionInCodeOccured;
            }
        }

        protected ModbusErrorCode ReadStatuses(Byte functionNumber, Byte rtuAddress, UInt16 startAddress, ref bool[] statusesValues)
        {
            if (!IsConnected)
                return ModbusErrorCode.codeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.codeInvalidSlaveAddress;

            if ((statusesValues.Length > 2000) || (statusesValues.Length < 1) || (startAddress + statusesValues.Length > UInt16.MaxValue))
                return ModbusErrorCode.codeInvalidRequestedSize;

            Byte[] recievedPacket = null;

            ushort expectedRecievedPacketSize = (ushort)(5 + (statusesValues.Length + 7) / 8);
            
            try
            {
                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, startAddress, (UInt16)statusesValues.Length);                

                if (TxRxMessage(sendPacket, ref recievedPacket, expectedRecievedPacketSize))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1], expectedRecievedPacketSize);
                    if (errorCode == ModbusErrorCode.codeOK)
                    {
                        Byte[] packetData = new Byte[recievedPacket.Length - 5];
                        Array.Copy(recievedPacket, 3, packetData, 0, packetData.Length);

                        try
                        {
                            ProcessDiscreetData(packetData, ref statusesValues);                            
                        }
                        catch (Exception ex)
                        {
                            if (_logExceptionModbusRTU != null)
                                _logExceptionModbusRTU(ex);
                            return ModbusErrorCode.codeExceptionInCodeOccured;
                        }
                        //TODO here we need to log the StatusString
                        return ModbusErrorCode.codeOK;
                    }
                    
                    //TODO here we need to log the StatusString
                    return errorCode;
                }                
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.codeTimeout;
                if (StatusString.Contains("Error: Invalid response length"))
                {
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                }
                return ModbusErrorCode.codeErrorSendingPacket;               
            }
            catch (Exception ex)
            {
                if (_logExceptionModbusRTU != null)
                    _logExceptionModbusRTU(ex);
                return ModbusErrorCode.codeExceptionInCodeOccured;
            }            
        }

        protected ModbusErrorCode ForceSingle(Byte functionNumber, Byte rtuAddress, UInt16 forceAddress, object value)
        {
            if (!IsConnected)
                return ModbusErrorCode.codeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.codeInvalidSlaveAddress;
            if ((value.GetType() != typeof(Int16))&&(value.GetType() != typeof(UInt16))&&(value.GetType() != typeof(bool)))
                return ModbusErrorCode.codeInvalidInputArgument;

            Byte[] recievedPacket = null;

            UInt16[] forcedValue = new ushort[1] { 0 };
            if (value.GetType() == typeof(bool))
            {
                if ((bool)value)
                    forcedValue[0] = 0xFF00;
            }
            else if (value.GetType() == typeof(Int16))
            {
                forcedValue[0] = BitConverter.ToUInt16(BitConverter.GetBytes((Int16)value), 0);
            }
            else
                forcedValue[0] = (UInt16)value;
          
            try
            {                                
                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, forceAddress, 1, forcedValue);
                
                if (TxRxMessage(sendPacket, ref recievedPacket, 8))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1], 8);
                    if (errorCode == ModbusErrorCode.codeOK)
                    {
                        if (recievedPacket.SequenceEqual(sendPacket))
                        {
                            //TODO here we need to log the StatusString
                            return ModbusErrorCode.codeOK;
                        }
                        return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                    }                   
                    //TODO here we need to log the StatusString
                    return errorCode;                    
                }               
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.codeTimeout;
                if (StatusString.Contains("Error: Invalid response length"))                
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                
                return ModbusErrorCode.codeErrorSendingPacket;                
            }
            catch (Exception ex)
            {
                if (_logExceptionModbusRTU != null)
                    _logExceptionModbusRTU(ex);
                return ModbusErrorCode.codeExceptionInCodeOccured;        
            }            
        }

        protected ModbusErrorCode ForceMultiple(Byte functionNumber, Byte rtuAddress, UInt16 forceAddress, object[] values)
        {
            if (!IsConnected)
                return ModbusErrorCode.codeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.codeInvalidSlaveAddress;

            if(values.Length <= 0)
                return ModbusErrorCode.codeInvalidInputArgument;

            bool[]   boolValues   = new bool[values.Length];                       

            Type arrayType = values[0].GetType();

            UInt16[] forcedValues = new UInt16[values.Length];
            if (arrayType == typeof(bool))            
                Array.Resize(ref forcedValues,(values.Length + 15)/16);

            int i = 0;
            foreach (var value in values)
            {
                if (((value.GetType() != typeof(Int16)) && (value.GetType() != typeof(UInt16)) && (value.GetType() != typeof(bool)))||(arrayType != value.GetType()))
                    return ModbusErrorCode.codeInvalidInputArgument;
                if (arrayType == typeof (bool))
                    boolValues[i] = (bool) value;                
                if (arrayType == typeof(UInt16))
                    forcedValues[i] = (UInt16)value;
                if (arrayType == typeof(Int16))
                    forcedValues[i] = BitConverter.ToUInt16(BitConverter.GetBytes((Int16)value), 0); 
                i++;
            }

            if (arrayType == typeof(bool))
            {
                Byte[] temp = new Byte[forcedValues.Length*2];
                new BitArray(boolValues).CopyTo(temp, 0);                
                Buffer.BlockCopy(temp, 0, forcedValues, 0, temp.Length);
            }            

            Byte[] recievedPacket = null;           

            try
            {
                Byte[] sendPacket = MakePacket(rtuAddress, functionNumber, forceAddress, (UInt16)values.Length, forcedValues, true, arrayType == typeof(bool));
                
                if (TxRxMessage(sendPacket, ref recievedPacket, 8))
                {
                    ModbusErrorCode errorCode = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1], 8);
                    if (errorCode == ModbusErrorCode.codeOK)
                    {
                        Byte[] tmp = BitConverter.GetBytes((UInt16)values.Length);
                        Array.Reverse(tmp); 
                        if (BitConverter.ToUInt16(recievedPacket, 4) != BitConverter.ToUInt16(tmp, 0))
                            return ModbusErrorCode.codeInvalidResponse;
                        tmp = BitConverter.GetBytes(forceAddress);
                        Array.Reverse(tmp);
                        if (BitConverter.ToUInt16(recievedPacket, 2) != BitConverter.ToUInt16(tmp, 0))
                            return ModbusErrorCode.codeInvalidResponse;                                                      
                        //TODO here we need to log the StatusString
                        return ModbusErrorCode.codeOK;
                    }
                    
                    //TODO here we need to log the StatusString
                    return errorCode;                    
                }
                
                //TODO here we need to log the StatusString
                if (StatusString.Contains("Timeout"))
                    return ModbusErrorCode.codeTimeout;
                    
                if (StatusString.Contains("Error: Invalid response length"))
                    return ParseExceptionCode(functionNumber, rtuAddress, recievedPacket);
                
                return ModbusErrorCode.codeErrorSendingPacket;                
            }
            catch (Exception ex)
            {
                if (_logExceptionModbusRTU != null)
                    _logExceptionModbusRTU(ex);
                return ModbusErrorCode.codeExceptionInCodeOccured;
            }            
        }        
    }   
}
