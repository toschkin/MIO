using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{
    public class DeviceStatusesHeader
    {
        public bool ModbusChannel1Status { get; set; }
        public bool ModbusChannel2Status { get; set; }
        public bool ModbusChannel3Status { get; set; }
        public bool RouterModuleStatus { get; set; }
        public bool DIModuleStatus { get; set; }
        public bool DOModuleStatus { get; set; }        
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
        /// <value>
        /// Программный модуль	№ бита
        /// MODBUS канал №1	    0
        /// MODBUS канал №2	    1
        /// MODBUS канал №3	    2
        /// ТС	                3
        /// ТУ	                4
        /// Маршрутизация	    5
        /// 
        /// 0 – модуль функционирует нормально;
        /// 1 –  (конфигурация модуля выполнена с коллизиями)/(ошибка модуля). 
        /// </value>        
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 GlobalStatusRegisterValue
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
                    if (field.GetCustomAttributes(typeof(ModbusPropertyAttribute), false).Length > 0)//except itself
                        continue;
                    field.SetValue(this, (value & (1 << i++)) != 0, null);
                }
            }
        }        
    }
}
