using System.Collections.Generic;

namespace MIOConfig.InternalLayer
{
    public interface IDeviceModule
    {
        List<object> ToList();
        bool FromList(List<object> listOfConfigurationItems);
    }
}