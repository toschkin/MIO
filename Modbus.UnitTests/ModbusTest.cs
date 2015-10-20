using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO.Ports;
using System.IO;
using Modbus.Core;
using Tech.CodeGeneration;
using System.Threading;
using System.Reflection;


namespace Modbus.UnitTests
{
    class MyClass
    {
        public MyClass()
        {
            a = 0;
            b = 0;
            c = 0.0f;
            d = 0.0;
        }        
        public Int16 a { get; set; }
        public Int32 b { get; set; }
        public Single c { get; set; }
        public Double d { get; set; }       
    }
    class MyClassWithPrivateProperty
    {
        public MyClassWithPrivateProperty()
        {
            a = 0;
            b = 0;
            c = 0.0f;
            d = 0.0;
        }
        private Int16 a { get; set; }
        public Int32 b { get; set; }
        public Single c { get; set; }
        public Double d { get; set; }
    }
    public static class TestHelper
    {
        public static void TestPropertySettingItsValueTo1IfParamIsLessThan1(RawSerialProtocol prot, string propertyName,int testValue)
        {
            PropertyInfo prop = typeof(RawSerialProtocol).GetProperty(propertyName);
            
            // create an open "getter" delegate
            Func<RawSerialProtocol, int> getForAnyRawSerialProtocolIntProp = (Func<RawSerialProtocol, int>)
                Delegate.CreateDelegate(typeof(Func<RawSerialProtocol, int>), null,
                    prop.GetGetMethod());

            Action<RawSerialProtocol, int> setForAnyRawSerialProtocolIntProp = (Action<RawSerialProtocol, int>)
            Delegate.CreateDelegate(typeof(Action<RawSerialProtocol, int>), null,
                prop.GetSetMethod());

            setForAnyRawSerialProtocolIntProp(prot, testValue);
            Assert.AreEqual(1, getForAnyRawSerialProtocolIntProp(prot));
        }

        public static void TestLogFileOutput(RawSerialProtocol prot, String testedMethod)
        {
            prot.LogsPath = @"c:\";
            prot.SaveExceptionsToLog = true;
            FileInfo logFileInfo = new FileInfo(prot.LogsPath + "ModbusCoreExceptions.txt");
            if (logFileInfo.Exists)
            {
                long lengthBeforeAddException = logFileInfo.Length;

                var code = CodeGenerator.CreateCode<bool>(testedMethod,
                                                            new[] { "System.IO.Ports" }, new[] { "System.dll" },
                                                            new CodeParameter("prot", typeof(Modbus.Core.RawSerialProtocol)));
                code.Execute(prot);

                logFileInfo.Refresh();
                if (lengthBeforeAddException >= logFileInfo.Length)
                    Assert.Fail();
            }
            else
            {
                var code = CodeGenerator.CreateCode<bool>(@"prot." + testedMethod,
                                                                new[] { "System.IO.Ports" }, new[] { "System.dll" },
                                                                new CodeParameter("prot", typeof(Modbus.Core.RawSerialProtocol)));
                code.Execute(prot);

                FileInfo logFileInfo2 = new FileInfo(prot.LogsPath + "ModbusCoreExceptions.txt");
                if (!logFileInfo2.Exists)
                    Assert.Fail();
            }
        }
    }
    class RawSerialProtocolTest
    {
        const String portNameForTest = "COM6";
        const String loopPortNameModsim = "COM8";
        const String loopPortNameM = "COM6";
        const String loopPortNameS = "COM7";
       
        [Test]
        public void DefaultCtorShouldSetProperties()
        {
            RawSerialProtocol prot = new RawSerialProtocol();           
            Assert.AreEqual(false, prot.IsConnected);            
            Assert.AreEqual(20, prot.SilentInterval);
            Assert.That(prot.StatusString, Is.EqualTo("Not connected"));            
            Assert.AreEqual(SerialPortPin.None, prot.TangentaPin);
            Assert.AreEqual(100, prot.TangentaSetPinTimePeriodMsec);            
            Assert.AreEqual(false, prot.SaveExceptionsToLog);
            Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, prot.LogsPath);
            Assert.AreEqual(1, prot.Retries);
        }        

        #region Properties Tests
        
        [Test]
        [ExpectedException()]
        [TestCase(@"C::\")]
        [TestCase(null)]
        [TestCase(@"")]
        public void LogsPath_ShouldThrowExceptionOnInvalidPath(string path)
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            prot.LogsPath = path;
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-100)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Properties_ShouldSetValueTo1AndThrowExceptionOnLessThan1ms(int testValue)
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            TestHelper.TestPropertySettingItsValueTo1IfParamIsLessThan1(prot, "SilentInterval", testValue);
            TestHelper.TestPropertySettingItsValueTo1IfParamIsLessThan1(prot, "TangentaSetPinTimePeriodMsec", testValue);
            TestHelper.TestPropertySettingItsValueTo1IfParamIsLessThan1(prot, "Retries", testValue);
        }      
        #endregion

        #region Connect Tests
        [Test]
        public void Connect_ShouldReturnTrueOnSuccess()
        {           
            RawSerialProtocol prot = new RawSerialProtocol();
            Assert.AreEqual(true, prot.Connect(portNameForTest, 19200, 7, StopBits.One, Parity.Even, 2000,Handshake.None));           
            prot.Disconnect();
        }

        [Test]
        [TestCase("COM255")]
        [TestCase(null)]
        [TestCase("")]
        public void Connect_ShouldSetStatusStringToErrorAndSaveItToLog(string port)
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            prot.Connect(port);
            Assert.That(prot.StatusString, Is.StringContaining("Error"));           
            prot.Disconnect();
        }

        [Test]
        public void Connect_ShouldSaveErrorToLog()
        {           
            RawSerialProtocol prot = new RawSerialProtocol();

            TestHelper.TestLogFileOutput(prot, "prot.Connect(\"COM255\",9600,8,System.IO.Ports.StopBits.Two,System.IO.Ports.Parity.None,1000,System.IO.Ports.Handshake.None);");
            TestHelper.TestLogFileOutput(prot, "prot.Connect(\"" + portNameForTest + "\",-1,8,System.IO.Ports.StopBits.Two,System.IO.Ports.Parity.None,1000,System.IO.Ports.Handshake.None);");
            TestHelper.TestLogFileOutput(prot, "prot.Connect(\"" + portNameForTest + "\",0,8,System.IO.Ports.StopBits.Two,System.IO.Ports.Parity.None,1000,System.IO.Ports.Handshake.None);");
            TestHelper.TestLogFileOutput(prot, "prot.Connect(\"" + portNameForTest + "\",9600,88,System.IO.Ports.StopBits.Two,System.IO.Ports.Parity.None,1000,System.IO.Ports.Handshake.None);");
            TestHelper.TestLogFileOutput(prot, "prot.Connect(\"" + portNameForTest + "\",9600,System.Int32.MinValue,System.IO.Ports.StopBits.Two,System.IO.Ports.Parity.None,1000,System.IO.Ports.Handshake.None);");
            TestHelper.TestLogFileOutput(prot, "prot.Connect(\"" + portNameForTest + "\",9600,0,System.IO.Ports.StopBits.Two,System.IO.Ports.Parity.None,1000,System.IO.Ports.Handshake.None);");
            TestHelper.TestLogFileOutput(prot, "prot.Connect(\"" + portNameForTest + "\",9600,8,System.IO.Ports.StopBits.Two,System.IO.Ports.Parity.None,System.Int32.MinValue,System.IO.Ports.Handshake.None);");            
            prot.Disconnect();
            
        }               

        /*[Test]
        public void Connect_ShouldOpenPort()
        {                     
            RawSerialProtocol prot = new RawSerialProtocol();
            prot.Connect(portNameForTest);
            Assert.AreEqual(true,prot.IsConnected);
            Assert.That(prot.StatusString, Is.EqualTo("Connected"));
            prot.Disconnect();
            Assert.That(prot.StatusString, Is.EqualTo("Not connected"));
        }*/
        #endregion

        #region Disconnect Tests
        /*[Test]
        public void Disconnect_ShouldClosePort()
        {            
            RawSerialProtocol prot = new RawSerialProtocol();
            prot.Connect(portNameForTest);
            prot.Disconnect();
            Assert.AreEqual(false, prot.IsConnected);
            Assert.That(prot.StatusString, Is.EqualTo("Not connected"));
        }*/
        #endregion

        #region SendPacket Tests
        /*[Test]
        public void SendPacket_ShouldReturnTrueOnSuccessFalseOnError()
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            Assert.AreEqual(true,prot.Connect(portNameForTest));           
            Byte[] packet = {0xA,0xB,0xC,0xD,0xE,0xF};
            Boolean actualRetCode = prot.SendPacket(packet);
            Assert.AreEqual(true, actualRetCode);
            Assert.That(prot.StatusString, Is.EqualTo("OK"));    
            prot.Disconnect();
            actualRetCode = prot.SendPacket(packet);
            Assert.AreEqual(false, actualRetCode);
            Assert.That(prot.StatusString, Is.StringContaining("Error"));          
        }      
        [Test]
        public void SendPacket_ShouldSaveExceptionsToLog()
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            prot.Connect(portNameForTest);
            TestHelper.TestLogFileOutput(prot, @"Byte[] packet = null;
                                                        prot.SendPacket(packet);");
            prot.Disconnect();
        } */      
        #endregion 
        
        #region RecivePacket Tests
        /*[Test]
        [ExpectedException(typeof(TimeoutException))]
        public void RecivePacket_ShouldThrowExceptionOnTimeout()
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            Assert.AreEqual(true, prot.Connect(portNameForTest));
            Byte[] packet = null;
           
            Boolean actualRetCode = prot.RecivePacket(ref packet);
            Assert.That(prot.StatusString, Is.StringContaining("Timeout"));    
            prot.Disconnect();
        } 
        [Test]
        public void RecivePacket_ShouldReturnFalseOnError()
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            Byte[] packetSend = { 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };            
            Boolean actualRetCode = prot.RecivePacket(ref packetSend);
            Assert.AreEqual(false, actualRetCode);
            Assert.That(prot.StatusString, Is.StringContaining("Error"));            
            prot.Disconnect();
        }*/
        #endregion          

        #region TxRxMessage Tests
        /*[Test]
        public void TxRxMessageShouldWorkProperlyWithModbusSimulatorConnectedOnPort()
        {
            RawSerialProtocol protM = new RawSerialProtocol();
            protM.Connect(loopPortNameModsim, timeout: 100);

            
            Byte[] packetSend = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };

           
            Byte[] packetCompareRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44 };

            Byte[] packetRecieve = null;

            Assert.AreEqual(true,protM.TxRxMessage(packetSend, ref packetRecieve));
            Assert.AreEqual(packetCompareRecieve, packetRecieve);
            Assert.That(protM.StatusString, Is.StringContaining("OK"));            

            protM.Disconnect();          
        }
        [Test]        
        public void TxRxMessageShouldFailOnInvalidSendPacket()
        {
            RawSerialProtocol protM = new RawSerialProtocol();
            protM.Connect(loopPortNameM, timeout: 1500);

            Byte[] packetSend = null;
            Byte[] packetRecieve = null;

            Assert.AreEqual(false, protM.TxRxMessage(packetSend, ref packetRecieve));                       

            protM.Disconnect();
        }
       
        [Test]        
        public void TxRxMessageShouldFailOnTimeout()
        {
            RawSerialProtocol protM = new RawSerialProtocol();
            protM.Connect(loopPortNameM, timeout: 1500);           
            Byte[] packetSend = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };
            
            Byte[] packetCompareRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0x39, 0xB3 };

            Byte[] packetRecieve = null;

            Assert.AreEqual(false, protM.TxRxMessage(packetSend, ref packetRecieve));
            Assert.AreNotEqual(packetCompareRecieve, packetRecieve);
            Assert.That(protM.StatusString, Is.StringContaining("Timeout"));    

            protM.Disconnect();
        }

        [Test]
        public void TxRxMessageShouldPerformRetries()
        {
            RawSerialProtocol protM = new RawSerialProtocol();
            protM.Connect(loopPortNameM, timeout: 500);
            protM.Retries = 3;
            Byte[] packetSend = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };

            Byte[] packetCompareRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0x39, 0xB3 };

            Byte[] packetRecieve = null;

            Assert.AreEqual(false, protM.TxRxMessage(packetSend, ref packetRecieve));
            Assert.AreNotEqual(packetCompareRecieve, packetRecieve);
            Assert.That(protM.StatusString, Is.StringContaining("Timeout"));
            Assert.That(protM.StatusString, Is.StringContaining("Retries"));            
            protM.Disconnect();
        }         */   
        #endregion

        #region Loopback Tests
        /*[Test]
        public void SendRecieveTestOnLoobackPort()
        {
            RawSerialProtocol protM = new RawSerialProtocol();
            RawSerialProtocol protS = new RawSerialProtocol();
            protM.Connect(loopPortNameM);
            protS.Connect(loopPortNameS);
            Byte[] packetSend = { 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
            Byte[] packetRecieve = null;
            Boolean actualRetCode = protM.SendPacket(packetSend);
            Assert.AreEqual(true, actualRetCode);
            Assert.That(protM.StatusString, Is.EqualTo("OK"));            
            actualRetCode = protS.RecivePacket(ref packetRecieve);
            Assert.AreEqual(true, actualRetCode);
            Assert.That(protS.StatusString, Is.EqualTo("OK"));
            Assert.AreEqual(packetSend, packetRecieve);
            protM.Disconnect();
            protS.Disconnect();           
        } */      
        #endregion
    }
    class ModbusRTUProtocolTest
    {
        const String portNameForTest = "COM6";
        class ModbusDataRegisterUInt16
        {            
            public UInt16 address;
            public UInt16 value { get; set; }
        }
        class ModbusDataRegisterInt32
        {
            public Int32 address;
            public Int32 value { get; set; }
        }
              
        /*[Test]
        public void _ModbusRTUProtocol_ReadHoldingRegistersShouldReturnOKcodeOnSuccess()
        {         
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            prot.Connect(portNameForTest);            
            ushort[] registerValues = null;
            ModbusErrorCode code = prot.ReadHoldingRegisters<ushort>(1 ,0 ,1,ref registerValues);
            Assert.AreEqual(ModbusErrorCode.codeOK, code);
            prot.Disconnect();
            
        }*/
        [Test]              
        public void AddCRCShouldAddCRCToPaccket()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };
            prot.AddCRC(ref packet);
            Byte[] packetCompare = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };
            Assert.AreEqual(8, packet.Length);
            Assert.AreEqual(packetCompare, packet);                        
        }
        [Test] 
        [ExpectedException(typeof(ArgumentNullException))]     
        public void AddCRCShouldThrowOnNullArgument()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet
            Byte[] packetNull = null;
            prot.AddCRC(ref packetNull);                                   
        }
        [Test] 
        [ExpectedException(typeof(ArgumentOutOfRangeException))]     
        public void AddCRCShouldThrowOnGreaterThanMaxSizePacket()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //invalid length packet (exceeds max length)
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };
            Array.Resize<Byte>(ref packet, 255);
            prot.AddCRC(ref packet);                            
        }
        [Test] 
        [ExpectedException(typeof(ArgumentOutOfRangeException))]     
        public void AddCRCShouldThrowOnLessThanMinSizePacket()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //invalid length packet (less than min length)
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };
            Array.Resize<Byte>(ref packet, 2);
            prot.AddCRC(ref packet);                            
        }                              
        [Test]
        public void CheckCRCShouldWorkProperlyOnVariousTypesOfPacket()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet            
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };
            bool bRetCode = prot.CheckCRC(packet);            
            Assert.AreEqual(true, bRetCode);                      
            //null packet
            Byte[] packetNull = null;
            bRetCode = prot.CheckCRC(packetNull);
            Assert.AreEqual(false, bRetCode);
            //invalid length packet (exceeds max length)
            Array.Resize<Byte>(ref packet, 270);
            bRetCode = prot.CheckCRC(packet);
            Assert.AreEqual(false, bRetCode);           
            //invalid length packet (less than min length)
            Array.Resize<Byte>(ref packet, 2);
            bRetCode = prot.CheckCRC(packet);
            Assert.AreEqual(false, bRetCode);           
        }

        [Test]
        public void CheckPacketShouldReturnOKCodeOnValidPacket()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet                      
            Byte[] packetRecieve = {0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44};
            ModbusErrorCode err = prot.CheckPacket(packetRecieve, 0x01, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeOK, err);
        }

        public void CheckPacketShouldReturncodeInvalidPacketLengthOnNullPacket()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();     
            //null packet
            Byte[] packetNull = null;
            ModbusErrorCode err = prot.CheckPacket(packetNull, 0x01, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidPacketLength, err);
        }

        public void CheckPacketShouldReturncodeInvalidFunctionOnInvalidFuncCode()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();          
            Byte[] packetRecieve = {0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44};
            //invalid function code
            ModbusErrorCode err = prot.CheckPacket(packetRecieve, 0x01, 0x02, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidFunction, err);
        }

        public void CheckPacketShouldReturncodeInvalidSlaveAddressOnInvalidSlaveAddress()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();   
            Byte[] packetRecieve = {0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44};
            //invalid slave address
            ModbusErrorCode err = prot.CheckPacket(packetRecieve, 0x02, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidSlaveAddress, err);
        }
        public void CheckPacketShouldReturncodeInvalidPacketLengthInApropriateConditionsOnInvalidPacketLength()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44 };
            //invalid length packet
            Array.Resize<byte>(ref packetRecieve,6);
            ModbusErrorCode err = prot.CheckPacket(packetRecieve, 0x01, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidPacketLength, err);
        }
        [Test]
        public void MakePacket_ShouldCreateModbusPacketOfApropriateLengthAndContents()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetSendCompare = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };
            Byte[] packetSend = null;
            Byte slaveAddr = 1;
            Byte funcCode = 3;
            UInt16 startAddress = 100;
            UInt16 quantity = 1;
            packetSend = prot.MakePacket(slaveAddr, funcCode, startAddress, quantity);
            Assert.AreEqual(packetSendCompare, packetSend);
        }
        [Test]
        public void ProcessData_ShouldProcessRawPacketBytesToApropriateValuesOfApropriateTypeIntoOutputArrayAndReturnTrueOnSuccess()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();                                            
           
            Byte[] packetRawData = {   0x20, //32
                                       0xFE, //-2
                                       0xFF, 0xFF, //-1
                                       0x01, 0xF4, //500                                    
                                       0x49, 0x96, 0x02, 0xD2, //1234567890
                                       0xF8, 0xA4, 0x32, 0xEB, //-123456789
                                       0x42, 0xF6, 0xE6, 0x66, //123.45       
                                       0x11,0x22,0x10,0xF4,0xB2,0xD2,0x30,0xA2,//1234567891011121314
                                       0xEE,0xDD,0xEF,0x0B,0x4D,0x2D,0xCF,0x5E,//-1234567891011121314
                                       0x40,0xc8,0x1c,0xd6,0xe6,0x85,0xdb,0x77,//12345.67891     
                                       0x0, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x11, 0x22, 0x10, 0xF4, 0x7D, 0xE9, 0x81, 0x15//1234567890.123456789
                                    };
            object[] arrayValues = { (Byte)0, (SByte)0, new ModbusDataRegisterUInt16(), (UInt16)0, (UInt32)0, new ModbusDataRegisterInt32(), (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
            bool ret = prot.ProcessData(packetRawData, ref arrayValues);

            Assert.AreEqual(true, ret);
            Assert.AreEqual(32, arrayValues[0]);
            Assert.AreEqual(-2, arrayValues[1]);
            Assert.AreEqual(-1, ((ModbusDataRegisterUInt16)arrayValues[2]).value);
            Assert.AreEqual(500, arrayValues[3]);
            Assert.AreEqual(1234567890, arrayValues[4]);
            Assert.AreEqual(-123456789, ((ModbusDataRegisterInt32)arrayValues[5]).value);
            Assert.AreEqual(123.45f, (Single)arrayValues[6], 0.001f);
            Assert.AreEqual(1234567891011121314, arrayValues[7]);
            Assert.AreEqual(-1234567891011121314, arrayValues[8]);
            Assert.AreEqual(12345.67891, (Double)arrayValues[9]);
            Assert.AreEqual(1234567890.123456789m, arrayValues[10]);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ProcessData_ShouldThrowIfNotValueTypeElementsAreRequestedInOutputArray()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                     0x01, 0xF4, //500
                                     0x00, 0x00, //0
                                     0x49, 0x96, 0x02, 0xD2, //1234567890
                                     };
            object[] rtuData = { (String)"aaa", new ModbusDataRegisterUInt16(), new ModbusDataRegisterUInt16(), new UInt32() };
            bool ret = prot.ProcessData(packetRawData, ref rtuData);                        
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessData_ShouldThrowOnNullInputArgument()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            object[] rtuData = { new Int16(), new ModbusDataRegisterUInt16(), new ModbusDataRegisterUInt16(), new UInt32() };
            prot.ProcessData(null, ref rtuData);     
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessData_ShouldThrowOnNullOutputArgument()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                     0x01, 0xF4, //500
                                     0x00, 0x00, //0
                                     0x49, 0x96, 0x02, 0xD2, //1234567890
                                     };
            object[] rtuData = new object[2];
            prot.ProcessData(packetRawData, ref rtuData);
        }
    }

    class SizeofHelperTest
    {
        [Test]
        public void SizeOfPublicPropertiesOfClass_ShouldReturnValidSize()
        {
            MyClassWithPrivateProperty cl = new MyClassWithPrivateProperty();
            UInt32 size = SizeofHelper.SizeOfPublicProperties(cl);
            Assert.AreEqual(16, size);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SizeOfPublicPropertiesOfClass_ShouldThrowOnNullArgument()
        {            
            UInt32 size = SizeofHelper.SizeOfPublicProperties(null);            
        }
    }

    class ModbusDataMappingHelperTest
    {        
        [Test]
        public void GetObjectPropertiesTypesArray_ShouldReturnArrayOfValidSizeAndValidTypes()
        {            		      
          	MyClass cl = new MyClass();
            Type[] arrayOfTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(cl);
            Assert.AreEqual(arrayOfTypes.Length, 4);            
            int i = 0;
            foreach (var item in cl.GetType().GetProperties())
	        {
                Assert.AreEqual(item.PropertyType, arrayOfTypes[i]);
                i++;
	        }            
        }
        [Test]               
        public void GetObjectPropertiesTypesArray_ShouldReturnArrayOfValidSizeAndValidTypesForArrayAsArgument()
        {
            object[] array = { new MyClass(), new MyClass() };

            int totalLength = 0;
            foreach (var item in array)
            {
                totalLength += item.GetType().GetProperties().Length;
            }
            Assert.AreEqual(8, totalLength);

            Type[] arrayOfTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(array);
           
            Assert.AreEqual(totalLength,arrayOfTypes.Length);
            int i = 0;
            for (int j = 0; j < array.Length; j++)
            {
                foreach (var item in array[j].GetType().GetProperties())
                {
                    Assert.AreEqual(item.PropertyType, arrayOfTypes[i]);
                    i++;
                }    
            }            
        }
        [Test]
        public void GetObjectPropertiesTypesArray_ShouldExtractOnlyPublicPropertiesFromArgument()
        {
            object[] array = { new MyClassWithPrivateProperty(), new MyClassWithPrivateProperty() };
            Type[] arrayOfTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(array);
            Assert.AreEqual(arrayOfTypes.Length, 6);       
        }
        [Test]
        public void GetObjectPropertiesTypesArray_ShouldExtractOnlyPublicPropertiesFromArgumentArray()
        {
            MyClassWithPrivateProperty cl = new MyClassWithPrivateProperty();
            Type[] arrayOfTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(cl);
            Assert.AreEqual(arrayOfTypes.Length, 3);
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetObjectPropertiesTypesArray_ShouldThrowOnNullArrayArgument()
        {
            MyClass[] cl = new MyClass[2];
            Type[] arrayOfTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(cl);    
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetObjectPropertiesTypesArray_ShouldThrowOnNullArgument()
        {
            Type[] arrayOfTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetObjectPropertiesValuesFromArray_ShouldThrowOnNullArgument1()
        {
            MyClass cl = new MyClass();
            object tmp = (object)cl;
            bool ret = ModbusDataMappingHelper.SetObjectPropertiesValuesFromArray(ref tmp, null);
        }

        [Test]        
        public void SetObjectPropertiesValuesFromArray_ShouldSetPropertiesToValuesFromArrayAndReturnTrueOnSuccess()
        {
            MyClass cl = new MyClass();
            object tmp = (object)cl;
            object[] arrayValues = { (Int16)(-12345), (Int32)(-123456), (Single)123.456f, (Double)1234567890.0 };
            bool ret = ModbusDataMappingHelper.SetObjectPropertiesValuesFromArray(ref tmp, arrayValues);
            Assert.AreEqual(true, ret);
            Assert.AreEqual(arrayValues[0], cl.a);
            Assert.AreEqual(arrayValues[1], cl.b);
            Assert.AreEqual(arrayValues[2], cl.c);
            Assert.AreEqual(arrayValues[3], cl.d);
        }
        [Test]
        public void SetObjectPropertiesValuesFromArray_ShouldSetOnlyPublicPropertiesToValuesFromArrayAndReturnTrueOnSuccess()
        {
            MyClassWithPrivateProperty cl = new MyClassWithPrivateProperty();
            object tmp = (object)cl;
            object[] arrayValues = { (Int32)(-123456), (Single)123.456f, (Double)1234567890.0 };
            bool ret = ModbusDataMappingHelper.SetObjectPropertiesValuesFromArray(ref tmp, arrayValues);
            Assert.AreEqual(true, ret);
            Assert.AreEqual(arrayValues[0], cl.b);
            Assert.AreEqual(arrayValues[1], cl.c);
            Assert.AreEqual(arrayValues[2], cl.d);            
        }
        [Test]
        public void SetObjectPropertiesValuesFromArray_ShouldReturnFalseIfFailed()
        {
            MyClass cl = new MyClass();
            object tmp = (object)cl;
            object[] arrayValues = { (Int32)(-12345), (Int32)(-123456), (Single)123.456f, (Double)1234567890.0 };
            bool ret = ModbusDataMappingHelper.SetObjectPropertiesValuesFromArray(ref tmp, arrayValues);
            Assert.AreEqual(false, ret);          
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtractValueFromArrayByType_ShouldThrowOnNonNumericTypeArgument()
        {
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                    0x01, 0xF4, //500
                                    0x00, 0x00, //0
                                    0x49, 0x96, 0x02, 0xD2, //1234567890
                                    };
            int packetCurrentPositionIndex = 6;
            object[] arrayValues = { (Int32)(-12345), (UInt32)(123456), (Single)123.456f, (Double)1234567890.0 ,"aaa"};
            ModbusDataMappingHelper.ExtractValueFromArrayByType(packetRawData, ref  packetCurrentPositionIndex, ref arrayValues[4]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExtractValueFromArrayByType_ShouldThrowOnNullTypeArgument()
        {
            int packetCurrentPositionIndex = 6;
            object[] arrayValues = { (Int32)(-12345), (UInt32)(123456), (Single)123.456f, (Double)1234567890.0 };
            ModbusDataMappingHelper.ExtractValueFromArrayByType(null, ref  packetCurrentPositionIndex, ref arrayValues[1]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExtractValueFromArrayByType_ShouldThrowOnNegativeIndexArgument()
        {
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                    0x01, 0xF4, //500
                                    0x00, 0x00, //0
                                    0x49, 0x96, 0x02, 0xD2, //1234567890
                                    };
            object[] arrayValues = { (Int32)(-12345), (UInt32)(123456), (Single)123.456f, (Double)1234567890.0};
            int packetCurrentPositionIndex = -6;
            ModbusDataMappingHelper.ExtractValueFromArrayByType(packetRawData, ref  packetCurrentPositionIndex, ref arrayValues[1]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExtractValueFromArrayByType_ShouldThrowOnOutboundingIndexArgument()
        {
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                    0x01, 0xF4, //500
                                    0x00, 0x00, //0
                                    0x49, 0x96, 0x02, 0xD2, //1234567890
                                    };
            object[] arrayValues = { (Int32)(-12345), (UInt32)(123456), (Single)123.456f, (Double)1234567890.0 };
            int packetCurrentPositionIndex = 7;
            ModbusDataMappingHelper.ExtractValueFromArrayByType(packetRawData, ref  packetCurrentPositionIndex, ref arrayValues[1]);
        }

        [Test]
        public void ExtractValueFromArrayByType_ShouldIncrementStartPosition()
        {
            Byte[] packetRawData = {   0x20, //32
                                       0xFE, //-2
                                       0xFF, 0xFF, //-1
                                       0x01, 0xF4, //500                                    
                                       0x49, 0x96, 0x02, 0xD2, //1234567890
                                    };
            object[] arrayValues = { (Byte)0, (SByte)0,(Int16)0, (UInt16)0, (UInt32)0, (Single)0.0 };
            
            int packetCurrentPositionIndex = 2;
            
            ModbusDataMappingHelper.ExtractValueFromArrayByType(packetRawData, ref packetCurrentPositionIndex,ref arrayValues[2]);
            Assert.AreEqual(4, packetCurrentPositionIndex);            
        }

        [Test]       
        public void ExtractValueFromArrayByType_ShouldSetApropriateValueFromInputArrayToOutputArgument()
        {
            Byte[] packetRawData = {   0x20, //32
                                       0xFE, //-2
                                       0xFF, 0xFF, //-1
                                       0x01, 0xF4, //500                                    
                                       0x49, 0x96, 0x02, 0xD2, //1234567890
                                       0xF8, 0xA4, 0x32, 0xEB, //-123456789
                                       0x42, 0xF6, 0xE6, 0x66, //123.45       
                                       0x11,0x22,0x10,0xF4,0xB2,0xD2,0x30,0xA2,//1234567891011121314
                                       0xEE,0xDD,0xEF,0x0B,0x4D,0x2D,0xCF,0x5E,//-1234567891011121314
                                       0x40,0xc8,0x1c,0xd6,0xe6,0x85,0xdb,0x77,//12345.67891     
                                       0x0, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x11, 0x22, 0x10, 0xF4, 0x7D, 0xE9, 0x81, 0x15//1234567890.123456789
                                    };
            object[] arrayValues = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0,(Decimal)0m };
            int packetCurrentPositionIndex = 0;
            for (int i = 0; i < arrayValues.Length; i++)
            {
                ModbusDataMappingHelper.ExtractValueFromArrayByType(packetRawData, ref packetCurrentPositionIndex, ref arrayValues[i]);                 
            }            
            Assert.AreEqual(32, arrayValues[0]);
            Assert.AreEqual(-2, arrayValues[1]);
            Assert.AreEqual(-1, arrayValues[2]);
            Assert.AreEqual(500, arrayValues[3]);
            Assert.AreEqual(1234567890, arrayValues[4]);
            Assert.AreEqual(-123456789, arrayValues[5]);
            Assert.AreEqual(123.45f, (Single)arrayValues[6],0.001f);
            Assert.AreEqual(1234567891011121314, arrayValues[7]);
            Assert.AreEqual(-1234567891011121314, arrayValues[8]);
            Assert.AreEqual(12345.67891, (Double)arrayValues[9]);
            Assert.AreEqual(1234567890.123456789m, arrayValues[10]);
        }
    }
    class ConversionHelperTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConvertBytesToFloat_ShouldThrowOnInvalidArgument()
        {
            Byte[] packetRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44 };
            float value = ConversionHelper.ConvertBytesToFloat(packetRecieve, 4, false);            
        }

        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertBytesToFloat_ShouldThrowOnNullArgument()
        {
            Byte[] packetRecieve = null;
            float value = ConversionHelper.ConvertBytesToFloat(packetRecieve, 4, false);            
        }
       
        [Test]       
        public void ConvertBytesToFloat_ShouldReturnValidFloationgNumber()
        {
            Byte[] packetRecieve = { 0x00, 0x00, 0x80, 0x3F };
            float value1 = ConversionHelper.ConvertBytesToFloat(packetRecieve, 0,false);            
            Byte[] packetRecieve2 = { 0x3F, 0x80, 0x00, 0x00 };
            float value2 = ConversionHelper.ConvertBytesToFloat(packetRecieve2, 0, true);
            Assert.AreEqual(value1, value2);
            Assert.AreEqual(1.0f, value2,0.1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConvertBytesToDouble_ShouldThrowOnInvalidArgument()
        {
            Byte[] packetRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44 };
            Double value = ConversionHelper.ConvertBytesToDouble(packetRecieve, 4, false);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertBytesToDouble_ShouldThrowOnNullArgument()
        {
            Byte[] packetRecieve = null;
            Double value = ConversionHelper.ConvertBytesToDouble(packetRecieve, 4, false);
        }

        [Test]
        public void ConvertBytesToDouble_ShouldReturnValidFloationgNumber()
        {
            Byte[] packetRecieve =  { 0x77, 0xdb, 0x85, 0xe6, 0xd6, 0x1c, 0xc8, 0x40 };
            Double value1 = ConversionHelper.ConvertBytesToDouble(packetRecieve, 0, false);            
            Byte[] packetRecieve2 = { 0x40, 0xc8, 0x1c, 0xd6, 0xe6, 0x85, 0xdb, 0x77 };//12345.67891
            Double value2 = ConversionHelper.ConvertBytesToDouble(packetRecieve2, 0, true);
            Assert.AreEqual(value1, value2,Double.Epsilon);
            Assert.AreEqual(12345.67891, value1, Double.Epsilon);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConvertBytesToDecimal_ShouldThrowOnInvalidArgument()
        {
            Byte[] packetRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44 };
            Decimal value = ConversionHelper.ConvertBytesToDecimal(packetRecieve, 4, false);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertBytesToDecimal_ShouldThrowOnNullArgument()
        {
            Byte[] packetRecieve = null;
            Decimal value = ConversionHelper.ConvertBytesToDecimal(packetRecieve, 4, false);
        }

        [Test]
        public void ConvertBytesToDecimal_ShouldReturnValidFloationgNumber()
        {
            Byte[] packetRecieve = { 0x15, 0x81, 0xE9, 0x7D, 0xF4, 0x10, 0x22, 0x11, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x9, 0x0 };//1234567890.123456789
            Decimal value1 = ConversionHelper.ConvertBytesToDecimal(packetRecieve, 0, false);
            Byte[] packetRecieve2 = { 0x0, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x11, 0x22, 0x10, 0xF4, 0x7D, 0xE9, 0x81, 0x15 };//1234567890.123456789
            Decimal value2 = ConversionHelper.ConvertBytesToDecimal(packetRecieve2, 0, true);
            Assert.AreEqual(value1, value2);
            Assert.AreEqual(1234567890.123456789, value1);
        }
        
    }
    class ModbusLoggerTest
    {
        [Test]
        public void CtorShouldSetDefaultValues()
        {
            ModbusLogger log = new ModbusLogger();
            Assert.AreEqual(@"${basedir}\", log.ExceptionLogDir);            
        }
        
        [Test]
        public void ShouldSaveExceptionsToProperFile()
        {
            ModbusLogger log = new ModbusLogger();
            RawSerialProtocol prot = new RawSerialProtocol();
            try
            {
                prot.TangentaSetPinTimePeriodMsec = -10;
            }
            catch (Exception exception)
            {                
                log.ExceptionLogDir = @"c:\";
                Assert.AreEqual(@"c:\", log.ExceptionLogDir);
                Assert.AreEqual(@"c:\ModbusCoreExceptions.txt", log.ExceptionLogFilePath);
                FileInfo logFileInfo = new FileInfo(log.ExceptionLogFilePath);
                if (logFileInfo.Exists)
                {
                    long lengthBeforeAddException = logFileInfo.Length;
                    log.SaveException(exception);
                    logFileInfo.Refresh();
                    if (lengthBeforeAddException >= logFileInfo.Length)
                        Assert.Fail();
                }
                else
                {
                    log.SaveException(exception);
                    FileInfo logFileInfo2 = new FileInfo(log.ExceptionLogFilePath);
                    if (!logFileInfo2.Exists)
                        Assert.Fail();
                }                
            }            
        }
    }
}
