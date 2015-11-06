using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Reflection;
using EnumExtension;

namespace Modbus.Core
{
    public enum ModbusErrorCode
    {
        [EnumDescription("OK")]
        CodeOk = 0x0000,
        [EnumDescription("Timeout error")]
        CodeTimeout = 0x0001,
        [EnumDescription("Not Connected error")]
        CodeNotConnected = 0x0002,
        [EnumDescription("CRC error")]
        CodeCrcError = 0x0003,
        [EnumDescription("Invalid slave address")]
        CodeInvalidSlaveAddress = 0x0004,
        [EnumDescription("Invalid function")]
        CodeInvalidFunction = 0x0005,
        [EnumDescription("Invalid requested size")]
        CodeInvalidRequestedSize = 0x0006,
        [EnumDescription("Invalid packet length")]
        CodeInvalidPacketLength = 0x0007,
        [EnumDescription("Invalid output array")]
        CodeInvalidOutputArray = 0x0008,
        [EnumDescription("Error when sending packet")]
        CodeErrorSendingPacket = 0x0009,
        [EnumDescription("Invalid input argument")]
        CodeInvalidInputArgument = 0x000A,
        [EnumDescription("Invalid response from slave")]
        CodeInvalidResponse = 0x000B,
        [EnumDescription("EXCEPTION in code")]
        CodeExceptionInCodeOccured = 0x000C,
        [EnumDescription("Exception code: Illegal Function")]
        CodeIllegalFunction = 0x8001,
        [EnumDescription("Exception code: Illegal Data Address")]
        CodeIllegalDataAddress = 0x8002,
        [EnumDescription("Exception code: Illegal Data Value")]
        CodeIllegalDataValue = 0x8003,
        [EnumDescription("Exception code: Slave Device Failure")]
        CodeSlaveDeviceFailure = 0x8004,
        [EnumDescription("Exception code: ACK")]
        CodeAck = 0x8005,
        [EnumDescription("Exception code: Slave Device Busy")]
        CodeSlaveDeviceBusy = 0x8006,
        [EnumDescription("Exception code: NACK")]
        CodeNack = 0x8007,
        [EnumDescription("Exception code: Memory Parity Error")]
        CodeMemoryParityError = 0x8008,
        [EnumDescription("Exception code: Gateway Path Unavailable")]
        CodeGatewayPathUnavailable = 0x800A,
        [EnumDescription("Exception code: Gateway Target Device Failed To Respond")]
        CodeGatewayTargetDeviceFailedToRespond = 0x800B       
    }   
}
