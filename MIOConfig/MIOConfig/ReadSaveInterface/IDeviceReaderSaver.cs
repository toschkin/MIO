using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIOConfig
{
    public interface IDeviceReaderSaver
    {
        ReaderSaverErrors SaveDeviceConfiguration(Device configuration);
        ReaderSaverErrors ReadDeviceConfiguration(ref Device configuration);
    }
}
