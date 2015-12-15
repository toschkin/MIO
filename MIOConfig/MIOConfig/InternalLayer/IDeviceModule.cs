using System.Collections.Generic;

namespace MIOConfig
{
    public interface IDeviceModule
    {
        List<object> ToList();
        bool FromList(List<object> listOfConfigurationItems);
    }
}