﻿using System;

namespace MIOConfig
{
    public delegate void ErrorTracer(string errorMessage);

    enum OutputTypes
    {
        NoOutput =0,
        SingleOutput=1,
        DoubleOutput =2,
        ParallelChannel=3
   }
    public static class Definitions
    {
        public const Byte MODBUS_MASTER_PROTOCOL = 0;
        public const Byte MODBUS_SLAVE_PROTOCOL = 1;
        public const UInt16 USER_REGISTERS_OFFSET = 0;
        public const UInt16 DEVICE_STATE_OFFSET = 500;
        public const UInt16 DEVICE_STATE_HEADER_SIZE = 2;
        public const UInt16 DEVICE_DI_MODULE_MAP_SIZE = 9;
        public const UInt16 DEVICE_DO_MODULE_MAP_SIZE = 13;
        public const UInt16 DEVICE_ROUTER_MODULE_MAP_SIZE = 1;
        public const UInt16 DEVICE_AI_MODULE_MAP_SIZE = 7;
        public const UInt16 CONFIGURATION_READ_OFFSET = 1000;
        public const UInt16 CONFIGURATION_WRITE_OFFSET = 1006;

        public const UInt16 DO_COIL_STATE_OFF = 0x0000;
        public const UInt16 DO_COIL_STATE_ON = 0xFF00;
        public const UInt16 DO_COIL_STATE_INDEF = 0xFFFF;        
    }
}