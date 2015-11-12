using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIOConfig
{
    public interface IDeviceReaderSaver
    {
        bool SaveDeviceConfiguration(List<object> configurationItems);
        bool ReadDeviceConfiguration(ref List<object> configurationItems);
    }
}
