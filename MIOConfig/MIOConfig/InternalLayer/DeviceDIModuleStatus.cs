﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    public class DeviceDIModuleStatus
    {
        public bool ErrorCurrent;
        public bool ErrorVoltage;
        public bool ErrorCheck;
        public bool Reserve3;
        public bool Reserve4;
        public bool Reserve5;
        public bool Reserve6;
        public bool Reserve7;
        public bool Reserve8;
        public bool Reserve9;
        public bool Reserve10;
        public bool Reserve11;
        public bool Reserve12;
        public bool Reserve13;
        public bool Reserve14;
        public bool Reserve15;
        /// <summary>
        /// Holding regs|addr.: 505|count: 1
        /// </summary>     
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 StatusRegister
        {
            get
            {
                int numeral = 0;
                Byte i = 0;
                foreach (var field in GetType().GetFields())
                {
                    if ((bool)field.GetValue(this))
                        numeral |= 1 << i;
                    i++;
                }
                return (UInt16)numeral;
            }
            set
            {
                Byte i = 0;
                foreach (var field in GetType().GetFields())
                {
                    field.SetValue(this, (value & (1 << i++)) != 0);
                }
            }
        }
    }
}