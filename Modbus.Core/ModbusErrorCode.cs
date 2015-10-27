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
        codeOK = 0x0000,
        [EnumDescription("Timeout error")]
        codeTimeout = 0x0001,
        [EnumDescription("Not Connected error")]
        codeNotConnected = 0x0002,
        [EnumDescription("CRC error")]
        codeCRCError = 0x0003,
        [EnumDescription("Invalid slave address")]
        codeInvalidSlaveAddress = 0x0004,
        [EnumDescription("Invalid function")]
        codeInvalidFunction = 0x0005,
        [EnumDescription("Invalid requested size")]
        codeInvalidRequestedSize = 0x0006,
        [EnumDescription("Invalid packet length")]
        codeInvalidPacketLength = 0x0007,
        [EnumDescription("Invalid output array")]
        codeInvalidOutputArray = 0x0008,
        [EnumDescription("Error when sending packet")]
        codeErrorSendingPacket = 0x0009,
        [EnumDescription("Invalid input argument")]
        codeInvalidInputArgument = 0x000A,
        [EnumDescription("Exception code: Illegal Function")]
        codeIllegalFunction = 0x8001,
        [EnumDescription("Exception code: Illegal Data Address")]
        codeIllegalDataAddress = 0x8002,
        [EnumDescription("Exception code: Illegal Data Value")]
        codeIllegalDataValue = 0x8003,
        [EnumDescription("Exception code: Slave Device Failure")]
        codeSlaveDeviceFailure = 0x8004,
        [EnumDescription("Exception code: ACK")]
        codeACK = 0x8005,
        [EnumDescription("Exception code: Slave Device Busy")]
        codeSlaveDeviceBusy = 0x8006,
        [EnumDescription("Exception code: NACK")]
        codeNACK = 0x8007,
        [EnumDescription("Exception code: Memory Parity Error")]
        codeMemoryParityError = 0x8008,
        [EnumDescription("Exception code: Gateway Path Unavailable")]
        codeGatewayPathUnavailable = 0x800A,
        [EnumDescription("Exception code: Gateway Target Device Failed To Respond")]
        codeGatewayTargetDeviceFailedToRespond = 0x800B       
    }   
}
