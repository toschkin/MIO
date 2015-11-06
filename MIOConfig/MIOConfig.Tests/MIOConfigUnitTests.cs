using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MIOConfig;
using Modbus.Core;

namespace MIOConfig.Tests
{
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
