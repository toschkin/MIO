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
        PGNG,
        TxD,
        RxD,
        DTR,
        GND,
        DSR,
        RTS,
        CTS,
        RI
    }
}
