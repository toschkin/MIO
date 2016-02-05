using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    [Serializable]
    public class DeviceModbusMasterQuery
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Type t = this.GetType();
            foreach (var prop in t.GetProperties())
            {
                sb.AppendFormat("{0}={1}\t", prop.Name, prop.GetValue(this,null));

            }
            return sb.ToString();
        }

        public DeviceModbusMasterQuery()
        {            
            SlaveAddress = 1;
            ModbusFuctionCode = 1;
        }
       
        public bool QueryConfigured
        {
            //get { return RegistersCount > 0; }            
            get; set;
        }
   
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 Reserved1 { get; set; }

        private byte _slaveAddress;
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte SlaveAddress
        {
            get { return _slaveAddress; }
            set
            {
                if ((value > 247) || (value < 1))
                {
                    _slaveAddress = 1;
                }
                else
                {
                    _slaveAddress = value;
                }
            }
        }

        private byte _modbusFuctionCode;
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public Byte ModbusFuctionCode
        {
            get { return _modbusFuctionCode; }
            set
            {
                if ((value > 4) || (value < 1))
                {
                    _modbusFuctionCode = 1;
                }
                else
                {
                    _modbusFuctionCode = value;
                }
            }
        }

        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 RegistersStartAddress { get; set; }

        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 RegistersCount { get; set; }

        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 RouteStartAddress { get; set; }       

        [ModbusProperty(Access = ModbusRegisterAccessType.AccessReadWrite)]
        public UInt16 QueryStatusAddress { get; set; }
    }
}
