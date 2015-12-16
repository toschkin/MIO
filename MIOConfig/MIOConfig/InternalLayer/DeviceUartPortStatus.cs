using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig
{   
    public class DeviceUartPortStatus
    {        
        /// <summary>
        /// Holding regs|addr.: 502...|count: 1
        /// </summary>     
        [ModbusProperty(Access = ModbusRegisterAccessType.AccessRead)]
        public UInt16 StatusRegister{ get; set; }
       
        public override string ToString()
        {
            switch (StatusRegister)
            {
                case 0:
                    return "Функционмирует нормально";
                case 1:
                    return "Исключение MODBUS №1";
                case 2:
                    return "Исключение MODBUS №2";
                case 3:
                    return "Исключение MODBUS №3";
                case 4:
                    return "Исключение MODBUS №4";
                case 5:
                    return "Исключение MODBUS №5";
                case 6:
                    return "Исключение MODBUS №6";
                case 7:
                    return "Исключение MODBUS №7";
                case 8:
                    return "Исключение MODBUS №8";
                case 9:
                    return "MODBUS_SLAVE: ошибка полей и/или контрольной суммы запроса";
                case 10:
                    return "MODBUS_MASTER таймаут ответа";
                case 11:
                    return "MODBUS_MASTER ошибка полей адреса устройства или функции в ответе";
                case 12:
                    return "MODBUS_MASTER ошибка контрольной суммы в ответе";
            }
            return "Неизвестный код ошибки";
        }
    }
}
