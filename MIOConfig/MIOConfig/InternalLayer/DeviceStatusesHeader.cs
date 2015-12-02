﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig.InternalLayer
{
    public class DeviceStatusesHeader
    {        
        /// <summary>
        /// Holding regs|addr.: 500 |count: 1| R/W
        /// </summary>
        /// <value>Регистр Global_ control используеться для апаратной перезагрузки устройства. Для выполнения перезагрузки нужно выполнить  2-х шаговую последовательность:
        ///1 - записть в регистр командой 6 MODBUS число 0х55FF;
        ///2 - записть в регистр командой 6 MODBUS число 0хFF55;
        ///Временной интервал между 1 и 2 должено составлять не не менее 0,5 секунды и не более 2.04 (+-0.03) секунд!!!
        ///</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 ControlRegisterValue { get; set; }

        /// <summary>
        /// Holding regs|addr.: 501 |count: 1| RO
        /// </summary>
        /// <value>Not implemented yet</value>        
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 GlobalStatusRegisterValue { get; set; }
    }
}
