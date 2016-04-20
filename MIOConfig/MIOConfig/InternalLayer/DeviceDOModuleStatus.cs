﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceDOModuleStatus
    {
        public bool Reserve0 { get; set; }
        public bool ErrorContactsSticking{ get; set; }
        public bool ErrorNoVoltage{ get; set; }
        public bool ErrorRelayContactsNotClosed{ get; set; }
        public bool ErrorPowerSwitchNotClosed{ get; set; }
        public bool ErrorPowerSwitchNotOpened{ get; set; }
        public bool ErrorRelayContactsNotOpened{ get; set; }
        public bool ErrorRelayCoils{ get; set; }
        public bool Reserve8{ get; set; }
        public bool Reserve9{ get; set; }
        public bool Reserve10{ get; set; }
        public bool Reserve11{ get; set; }
        public bool Reserve12{ get; set; }
        public bool Reserve13{ get; set; }
        public bool Reserve14{ get; set; }
        public bool Reserve15{ get; set; }
        /// <summary>
        /// Holding regs|addr.: 514,523-526|count: 1
        /// </summary>     
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 StatusRegister
        {
            get
            {
                int numeral = 0;
                Byte i = 0;
                foreach (var field in OrderedGetter.GetObjectPropertiesInDeclarationOrder(this))
                {
                    if (field.GetCustomAttributes(typeof(ModbusPropertyAttribute), false).Length > 0)//except itself
                        continue;

                    if ((bool)field.GetValue(this, null))
                        numeral |= 1 << i;
                    i++;
                }
                return (UInt16)numeral;
            }
            set
            {
                Byte i = 0;
                foreach (var field in OrderedGetter.GetObjectPropertiesInDeclarationOrder(this))
                {
                    if (field.GetCustomAttributes(typeof (ModbusPropertyAttribute), false).Length > 0) //except itself
                        continue;
                    field.SetValue(this, (value & (1 << i++)) != 0, null);
                }
            }
        }
    }
}
