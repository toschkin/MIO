using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Core
{
    /// <summary>
    /// Enumeration for serial port pins
    /// </summary>
    public enum SerialPortPin
    {
        None,
        Pgng,
        TxD,
        RxD,
        Dtr,
        Gnd,
        Dsr,
        Rts,
        Cts,
        Ri
    }
}
