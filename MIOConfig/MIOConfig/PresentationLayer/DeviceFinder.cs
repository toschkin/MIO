﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Core;

namespace MIOConfig.PresentationLayer
{
    public class DeviceFinder
    {
        private IModbus _protocol;   
        public DeviceFinder(IModbus protocol,Byte startSlaveAddress = 1,Byte endSlaveAddress = 247, UInt16 targetVersion=0)
        {
            _protocol = protocol;
            StartSlaveAddress = startSlaveAddress;
            EndSlaveAddress = endSlaveAddress;
            if (EndSlaveAddress < StartSlaveAddress)
                StartSlaveAddress = EndSlaveAddress;
            TargetVersion = targetVersion;
        }

        private Byte _startSlaveAddress;
        public Byte StartSlaveAddress {
            get { return _startSlaveAddress; }
            set
            {
                _startSlaveAddress = value;
                if (value <= 1)
                    _startSlaveAddress = 1;
                if (value >= 247)
                    _startSlaveAddress = 247;
            }
        }

        private Byte _endSlaveAddress;
        public Byte EndSlaveAddress
        {
            get { return _endSlaveAddress; }
            set
            {
                _endSlaveAddress = value;
                if (value <= 1)
                    _endSlaveAddress = 1;
                if (value >= 247)
                    _endSlaveAddress = 247;
            }
        }
        /// <summary>
        /// Version of Device to search, 0 - all versions
        /// </summary>
        public UInt16 TargetVersion { get; set; }
            
        public List<Device> FindDevices()
        {
            List<Device> returnedDevices = new List<Device>();
            ModbusReaderSaver reader = new ModbusReaderSaver(_protocol);
            for (Byte address = StartSlaveAddress; address <= EndSlaveAddress; address++)
            {
                Device device = new Device();
                reader.SlaveAddress = address;
                if (reader.CheckDeviceHeaderValidityAndInitConfiguration(device.Configuration, true) ==
                    ReaderSaverErrors.CodeOk)
                {
                    if (TargetVersion != 0)
                    {
                        if(TargetVersion == device.Configuration.HeaderFields.DeviceVersion)
                            returnedDevices.Add(device);
                    }
                    else
                        returnedDevices.Add(device);
                }
                    
            }
            return returnedDevices;
        }
    }
}