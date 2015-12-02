using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumExtension;

namespace MIOConfig
{
    public enum ReaderSaverErrors
    {
        [EnumDescription("OK")]
        CodeOk = 0,
        [EnumDescription("Invalid Device Header")]
        CodeInvalidDeviceHeader = 1,
        [EnumDescription("Not Compliant Device")]
        CodeNotCompliantDevice = 2,
        [EnumDescription("Modbus Communication Error")]
        CodeModbusCommunicationError = 3,
        [EnumDescription("Com Port Not Connected")]
        CodeComPortNotConnected = 4,
        [EnumDescription("Unknown Error")]
        CodeUnknownError = 5,
        [EnumDescription("Invalid Configuration Map Size")]
        CodeInvalidConfigurationSize = 6,
        [EnumDescription("Serialization Error")]
        CodeSerializationError = 7,
        [EnumDescription("Invalid Statuses Map Size")]
        CodeInvalidStatusesSize = 8,
        [EnumDescription("Module Is Absent")]
        CodeModuleIsAbsent = 9,
    }
}
