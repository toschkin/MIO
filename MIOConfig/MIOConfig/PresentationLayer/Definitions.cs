using System;

namespace MIOConfig
{
    public delegate void ErrorTracer(string errorMessage);    

    public static class Definitions
    {
        public const Byte MODBUS_MASTER_PRTOCOL = 0;
        public const Byte MODBUS_SLAVE_PRTOCOL = 1;
        public const UInt16 USER_REGISTERS_OFFSET = 0;
        public const UInt16 DEVICE_STATE_OFFSET = 500;
        public const UInt16 CONFIGURATION_READ_OFFSET = 1000;
        public const UInt16 CONFIGURATION_WRITE_OFFSET = 1006;

        public const UInt16 DO_COIL_STATE_OFF = 0x0000;
        public const UInt16 DO_COIL_STATE_ON = 0xFF00;
        public const UInt16 DO_COIL_STATE_INDEF = 0xFFFF;
    }
}