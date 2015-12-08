﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIOConfig.InternalLayer;

namespace MIOConfig
{
    public interface IDeviceReaderSaver
    {
        ReaderSaverErrors SaveDeviceConfiguration(DeviceConfiguration configuration);
        ReaderSaverErrors ReadDeviceConfiguration(ref DeviceConfiguration configuration);
    }
}