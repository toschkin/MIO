using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceModuleAIConfiguration
    {
        public DeviceModuleAIConfiguration()
        {
            Frequency = 50;
            ModuleOperation = 1;
        }
        private UInt16 _moduleOperation;
        /// <summary>
        /// Holding regs|R/W
        /// </summary>
        /// <value>0 - out of operation, 1 - in operation</value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 ModuleOperation
        {
            get { return _moduleOperation; }
            set { _moduleOperation = value > 1 ? (UInt16)1 : value; }
        }

        /// <summary>
        /// Holding regs R/W
        /// </summary>
        /// <value>range: 0..65535</value>
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 Frequency { get; set; }

        /// <summary>
        /// Holding regs R/W
        /// </summary>
        /// <value></value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 ResevedWord1 { get; set; }
        
        /// <summary>
        /// Holding regs R/W
        /// </summary>
        /// <value></value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 ResevedWord2 { get; set; }

        /// <summary>
        /// Holding regs R/W
        /// </summary>
        /// <value></value>
        /// 
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 ResevedWord3 { get; set; }
    }
}
