using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Core
{
    public enum ModbusErrorCode
    {
        codeOK = 0x0000,
        codeTimeout = 0x0001,
        codeNotConnected = 0x0002,
        codeCRCError = 0x0003,
        codeInvalidSlaveAddress = 0x0004,
        codeInvalidFunction = 0x0005,
        codeInvalidRegsSize = 0x0006,
        codeInvalidPacketLength = 0x0007,
        codeIllegalFunction = 0x8001,
        codeIllegalDataAddress = 0x8002,
        codeIllegalDataValue = 0x8003,
        codeSlaveDeviceFailure = 0x8004,
        codeACK = 0x8005,
        codeSlaveDeviceBusy = 0x8006,
        codeNACK = 0x8007,
        codeMemoryParityError = 0x8008,
        codeGatewayPathUnavailable = 0x800A,
        codeGatewayTargetDeviceFailedToRespond = 0x800B,
        
    }
}
