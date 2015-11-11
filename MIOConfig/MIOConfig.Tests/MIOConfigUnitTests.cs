using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MIOConfig;
using Modbus.Core;

namespace MIOConfig.Tests
{

    [TestClass]
    public class DeviceUARTPortConfigurationTests
    {
        [TestMethod]
        public void DeviceUARTPortConfiguration_AsignmentShouldWorkProperly()
        {
            DeviceUARTPortConfiguration devicePort1 = new DeviceUARTPortConfiguration();
            DeviceUARTPortConfiguration devicePort2 = new DeviceUARTPortConfiguration();
            devicePort1.PortSpeed = 1200;
            devicePort1.PortByteSize = 7;//error will be corrected
            devicePort2 = devicePort1;
            Assert.AreEqual(1200, devicePort2.PortSpeed);
            Assert.AreEqual(8, devicePort2.PortByteSize);
        }
    }

    [TestClass]
    public class DeviceConfigurationTimeTests
    {
        [TestMethod]
        public void DeviceConfigurationTime_RegisterValue_ShouldReturnProperRepresentationOfTime()
        {
            DeviceConfigurationTime deviceTime = new DeviceConfigurationTime();
            deviceTime.ConfigurationTime = new DateTime(2063,12,31,23,59,59);
            Assert.AreEqual(0xEFBBFF9F, deviceTime.RegisterValue);                        
        }

        [TestMethod]
        public void DeviceConfigurationTime_RegisterValue_ShouldSetProperRepresentationOfTime()
        {
            DeviceConfigurationTime deviceTime = new DeviceConfigurationTime { RegisterValue = 0xEFBBFF9F };
            Assert.AreEqual(new DateTime(2063, 12, 31, 23, 59, 59), deviceTime.ConfigurationTime);           
        }

        [TestMethod]
        public void DeviceConfigurationTime_RegisterValue_IsAModbusProperty()
        {
            DeviceConfigurationTime deviceTime = new DeviceConfigurationTime();
            Assert.AreEqual(4u,SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(deviceTime));
        }
    }
    [TestClass]
    public class DeviceConsistenceTests
    {
        [TestMethod]
        public void DeviceConsistence_RegisterValue_ShouldReturnProperRepresentationOfMembers()
        {
            DeviceConsistence device = new DeviceConsistence
            {
                ModuleModbusMaster = true,
                ModuleModbusSlave = true,
                ModuleRetranslator = true
            };
            Assert.AreEqual(0x8003,device.RegisterValue);                        
        }

        [TestMethod]
        public void DeviceConsistence_RegisterValue_ShouldSetProperRepresentationOfMembers()
        {
            DeviceConsistence device = new DeviceConsistence {RegisterValue = 0x8003};
            Assert.AreEqual(true, device.ModuleModbusMaster);
            Assert.AreEqual(true, device.ModuleModbusSlave);
            Assert.AreEqual(true, device.ModuleRetranslator);
        }

        [TestMethod]
        public void DeviceConsistence_RegisterValue_IsAModbusProperty()
        {
            DeviceConsistence device = new DeviceConsistence { RegisterValue = 0x8003 };
            Assert.AreEqual(2u,SizeofHelper.SizeOfPublicPropertiesWithModbusAttribute(device));
        }
    }
}
