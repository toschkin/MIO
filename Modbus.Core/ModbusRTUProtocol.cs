using System;
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
            : base()
        {

        } 
        /// <summary>
        /// Reads 16bit holding registers from Modbus device and convert them to array of values of appropriate type
        /// </summary>
        /// <typeparam name="T">Possible types are: UInt16,Int16,UInt32,Int32</typeparam>       
        /// <param name="rtuAddress">address of Modbus device (1..247)</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>
        /// <param name="registersCount">count of registers to be read (1..125)</param>
        /// <param name="registerValues">array of returned values converted to type T</param>
        /// <param name="reverseOrder">reorders bytes or registers in values</param>
        /// <example>
        /// <para>Reads 20 16-bit registers from modbus slave 1 starting at address 0 and converts them to UInt32 array of size 10. Value of the first 16-bit register will be placed into high word of first 32-bit array element,value of second - into low word and so on.</para>
        /// <code>
        /// ModbusRTUProtocol prot = new ModbusRTUProtocol();
        /// prot.Connect("COM1",9600);
        /// UInt32[] regs= null;
        /// prot.ReadHoldingRegisters&lt;UInt32&gt;(1, 0, 20, ref regs,true);
        /// </code>
        /// </example>
        /// <returns> ModbusErrorCode.codeOK on success (<see cref= Modbus.Core.ModbusErrorCode/> enum for details)</returns>
        public ModbusErrorCode ReadHoldingRegisters<T>(Byte rtuAddress, UInt16 startAddress, UInt16 registersCount, ref T[] registerValues, bool reverseOrder = false)
        {     
            Type typeOfArray = typeof(T);    
            if (!typeOfArray.IsValueType)
                throw new ArgumentException();

            if (!IsConnected)
                return ModbusErrorCode.codeNotConnected;
            if ((rtuAddress > 247) || (rtuAddress < 1))
                return ModbusErrorCode.codeInvalidSlaveAddress;
            if ((registersCount > 125)||(registersCount < 1))
                return ModbusErrorCode.codeInvalidRegsSize;

            Byte[] recievedPacket = null;
            
            Byte[] sendPacket = MakePacket(rtuAddress, 0x03, startAddress, registersCount);

            ushort expectedRecievedPacketSize = (ushort)(5 + registersCount*2);

            if (TxRxMessage(sendPacket, ref recievedPacket,expectedRecievedPacketSize))
            {
                ModbusErrorCode code = CheckPacket(recievedPacket, sendPacket[0], sendPacket[1], expectedRecievedPacketSize);
                if (code == ModbusErrorCode.codeOK)
                {       
                    /*Array.Resize<T>(ref registerValues, recievedPacket[2]/Marshal.SizeOf(typeOfArray));

                    for (int i = 0; i < registerValues.Length; i++)
                    {                             
                        if (registerValues is UInt16[])
                        {
                            UInt16 valUI16 = 0;
                            if (reverseOrder)
                                valUI16 = BitConverter.ToUInt16(recievedPacket, 3 + i * 2);
                            else
                                valUI16 = ConversionHelper.ReverseBytes(BitConverter.ToUInt16(recievedPacket, 3 + i * 2));
                            registerValues.SetValue(valUI16,i);
                        }
                    }*/
                    return ModbusErrorCode.codeOK;                    
                }
                else
                    return code;
            }
            else
            { 
                if(StatusString.Contains("Timeout"))
                    return ModbusErrorCode.codeTimeout;
            }
           
            throw new ArgumentException();
        }
        /// <summary>
        /// Adds two bytes of CRC16 to array representing Modbus protocol data packet passed in argument
        /// </summary>
        /// <param name="packet">data packet to which we need to add CRC16</param>
        ///  <exception cref="System.ArgumentNullException">Thrown if packet is null</exception>
        ///  <exception cref="System.ArgumentOutOfRangeException">Thrown if argument length &lt; 2 or &gt; 254</exception>
        public void AddCRC(ref Byte[] packet)
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
        public bool CheckCRC(Byte[] packet)
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
            else
                return false;
        }
        
        /// <summary>
        /// Validates recieved packet
        /// </summary>
        /// <param name="packetRecieved">Packet to validate</param>
        /// <param name="slaveAddress">Slave address which answer we expect to recieve</param>
        /// <param name="functionCode">Modbus function code which we expect to recieve</param>
        /// <param name="expectedPacketLength">Expected length of the recieved packet</param>
        /// <returns>ModbusErrorCode.codeOK on success, error code otherwise</returns>
        public ModbusErrorCode CheckPacket(Byte[] packetRecieved, Byte slaveAddress, Byte functionCode, Int32 expectedPacketLength)
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

        public bool ProcessData(Byte[] rawPacketData, ref object[] outputValues)
        {
            if(rawPacketData == null)
                throw new ArgumentNullException();

            UInt32 totalLengthInBytesOfRequestedData = 0;
            int currentIndexInPacketData = 0;
            int currentIndexInOutputValues = 0;
            foreach (var value in outputValues)
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (value.GetType().IsValueType)//here we will process simple (only numeric) types
                {
                    if (ModbusDataMappingHelper.IsNumericType(value.GetType()))
                    {
                        totalLengthInBytesOfRequestedData += (UInt32)Marshal.SizeOf(value);
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            return false;
                        //further processing
                        //outputValues[currentIndexInOutputValues].
                        //currentIndexInPacketData                        
                    }
                }
                else//here we will process properties (only numeric) from complex (class) types 
                {
                    try
                    {
                        Type[] arrayOfOutputTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(value);
                        totalLengthInBytesOfRequestedData += ModbusDataMappingHelper.SizeOfPublicProperties(value);
                        if (totalLengthInBytesOfRequestedData > rawPacketData.Length)
                            return false;
                        //further processing 
                        //SetObjectPropertiesValuesFromArray(ref object obj, object[] arrayValues)                                             
                    }
                    catch (ArgumentException)
                    {
                        throw;
                    }                   
                    catch (Exception)
                    {
                        return false;
                    }                    
                }
                currentIndexInOutputValues++;
            }
            return true;
            /*Type typeOfArray = typeof(T);            
            if (!typeOfArray.IsValueType)
                throw new ArgumentException();
            Array.Resize<T>(ref registerValues, packetRecieved[2] / Marshal.SizeOf(typeOfArray));*/
            
        }
        /// <summary>
        /// Creates Modbus packet
        /// </summary>
        /// <param name="slaveAddress">address of Modbus device (1..247)</param>
        /// <param name="functionCode">Modbus function code</param>
        /// <param name="startAddress">start address from which registers will be read (0..65535)</param>
        /// <param name="quantity">count of registers/coils to be read for functionCode: 1,2,3,4</param>
        /// <returns>created packed</returns>                
        public Byte[] MakePacket(Byte slaveAddress, Byte functionCode, UInt16 startAddress, UInt16 quantity)
        {            
            List<Byte> sendPacketList = new List<Byte>();
            sendPacketList.Add(slaveAddress);
            sendPacketList.Add(functionCode);
            sendPacketList.Add(BitConverter.GetBytes(startAddress).ElementAt<Byte>(1));
            sendPacketList.Add(BitConverter.GetBytes(startAddress).ElementAt<Byte>(0));
            sendPacketList.Add(BitConverter.GetBytes(quantity).ElementAt<Byte>(1));
            sendPacketList.Add(BitConverter.GetBytes(quantity).ElementAt<Byte>(0));
            Byte[] sendPacket = sendPacketList.ToArray();
            AddCRC(ref sendPacket);
            return sendPacket;
        }
    }   
}
