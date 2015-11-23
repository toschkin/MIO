using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MIOConfig;
using Modbus.Core;

namespace MIOConfig.Tests
{
    [TestClass]
    public class DeviceTests
    {
        [TestMethod]
        public void DeviceUartChannelsCount_Property_Set_ShouldResizeDeviceUartPortsList()
        {
            DeviceConfiguration deviceConfiguration = new DeviceConfiguration();
            deviceConfiguration.DeviceHeaderFields.DeviceUartChannelsCount = 5;
            Assert.AreEqual(5, deviceConfiguration.DeviceUartPorts.Count);
            deviceConfiguration.DeviceHeaderFields.DeviceUartChannelsCount = 0;
            Assert.AreEqual(1, deviceConfiguration.DeviceUartPorts.Count);
        }        
    }
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
}
