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

        /*public static void TestLogFileOutput(RawSerialProtocol prot, String testedMethod)
        {
            prot.ExceptionLogsPath = @"d:\aaaa\";
            prot.SaveExceptionsToLog = true;
            FileInfo logFileInfo = new FileInfo(prot.ExceptionLogsPath + "ModbusCoreExceptions.txt");
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

                FileInfo logFileInfo2 = new FileInfo(prot.ExceptionLogsPath + "ModbusCoreExceptions.txt");
                if (!logFileInfo2.Exists)
                    Assert.Fail();
            }
        }*/
    }
    
    class RawSerialProtocolTest
    {
        /*const String portNameForTest = "COM6";
        const String loopPortNameModsim = "COM8";
        const String loopPortNameM = "COM6";
        const String loopPortNameS = "COM7";*/
       
        [Test]
        public void DefaultCtorShouldSetProperties()
        {
            RawSerialProtocol prot = new RawSerialProtocol();           
            Assert.AreEqual(false, prot.IsConnected);            
            Assert.AreEqual(20, prot.SilentInterval);
            Assert.That(prot.StatusString, Is.EqualTo("Not connected"));            
            Assert.AreEqual(SerialPortPin.None, prot.TangentaPin);
            Assert.AreEqual(100, prot.TangentaSetPinTimePeriodMsec);            
            //Assert.AreEqual(false, prot.SaveExceptionsToLog);
            //Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, prot.ExceptionLogsPath);
            Assert.AreEqual(1, prot.Retries);
        }        

        #region Properties Tests
        
        /*[Test]
        [ExpectedException()]
        [TestCase(@"C::\")]
        [TestCase(null)]
        [TestCase(@"")]
        public void LogsPath_ShouldThrowExceptionOnInvalidPath(string path)
        {
            RawSerialProtocol prot = new RawSerialProtocol();
            prot.ExceptionLogsPath = path;
        }*/

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
        /*[Test]
        public void Connect_ShouldReturnTrueOnSuccess()
        {           
            RawSerialProtocol prot = new RawSerialProtocol();
            Assert.AreEqual(true, prot.Connect(portNameForTest, 19200, 7, StopBits.One, Parity.Even, 2000,Handshake.None));           
            prot.Disconnect();
        }*/

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

        /*[Test]
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
            
        } */              

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

    class ModbusRTUProtocolTest : ModbusRTUProtocol
    {
        /*const String portNameForTest = "COM6";               
        [Test]
        public void ReadRegisters_ShouldReturnOKcodeOnSuccess()
        {         
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            prot.Connect(portNameForTest);                        
            object[] modbusTestMap = { new ModbusDataPoint<Byte>(), 
                                     new ModbusDataPoint<SByte>(), 
                                     new ModbusDataPoint<Int16>(),                                      
                                     new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt32>(), 
                                     new ModbusDataPoint<Int32>(), 
                                     new ModbusDataPoint<Single>(), 
                                     new ModbusDataPoint<UInt64>(), 
                                     new ModbusDataPoint<Int64>(),  
                                     new ModbusDataPoint<Double>(), 
                                     new ModbusDataPoint<Decimal>()};
            ModbusErrorCode code = prot.ReadRegisters(3, 1, 0, ref modbusTestMap);
            Assert.AreEqual(ModbusErrorCode.codeOK, code);            
            prot.Disconnect();            
        }
        [Test]
        public void ReadRegisters_ShouldReturnModbusErrorCode_codeNotConnectedIfDisconnected()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();            
            object[] modbusTestMap = { new ModbusDataPoint<Byte>(), 
                                     new ModbusDataPoint<SByte>(), 
                                     };
            ModbusErrorCode code = prot.ReadRegisters(3, 1, 0, ref modbusTestMap);
            Assert.AreEqual(ModbusErrorCode.codeNotConnected, code);
        }
        [Test]
        [TestCase(0)]
        [TestCase(248)]
        public void ReadRegisters_ShouldReturnModbusErrorCode_codeInvalidSlaveIfAddressSlaveAddrIsOutOfRange(Byte address)
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            prot.Connect(portNameForTest);         
            object[] modbusTestMap = { new ModbusDataPoint<Byte>(), 
                                     new ModbusDataPoint<SByte>(), 
                                     };
            ModbusErrorCode code = prot.ReadRegisters(3, address, 0, ref modbusTestMap);
            Assert.AreEqual(ModbusErrorCode.codeInvalidSlaveAddress, code);
            prot.Disconnect();
        }
        [Test]                
        public void ReadRegisters_ShouldReturnModbusErrorCode_codeInvalidRequestedSizeIfItIsOutOfRange()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            prot.Connect(portNameForTest);         
            object[] modbusTestMap = { new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt16>() 
                                     };
            ModbusErrorCode code = prot.ReadRegisters(3, 1, 65533, ref modbusTestMap);
            Assert.AreEqual(ModbusErrorCode.codeInvalidRequestedSize, code);
            prot.Disconnect();
        }
        [Test]
        public void ReadRegisters_ShouldReturnModbusErrorCode_codeInvalidRequestedSizeIfItIsGreater125Regs()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            prot.Connect(portNameForTest);
            List<ModbusDataPoint<UInt16>> listDataPoints = new List<ModbusDataPoint<UInt16>>();
            for (int i = 0; i < 127; i++)
            {
                listDataPoints.Add(new ModbusDataPoint<ushort>());
            }
            object[] modbusTestMap = listDataPoints.ToArray();
            ModbusErrorCode code = prot.ReadRegisters(3, 1, 0, ref modbusTestMap);
            Assert.AreEqual(ModbusErrorCode.codeInvalidRequestedSize, code);
            prot.Disconnect();
        }
        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void ReadRegisters_ShouldThrowOnNullReferencesInArgument()
        {
            ModbusRTUProtocol prot = new ModbusRTUProtocol();
            prot.Connect(portNameForTest);
            List<ModbusDataPoint<UInt16>> listDataPoints = new List<ModbusDataPoint<UInt16>>();           
            listDataPoints.AddRange(new ModbusDataPoint<ushort>[10]);
            object[] modbusTestMap = listDataPoints.ToArray();
            ModbusErrorCode code = prot.ReadRegisters(3, 1, 0, ref modbusTestMap);            
            prot.Disconnect();
        }*/
        [Test]              
        public void AddCRC_ShouldAddCRCToPaccket()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };
            //prot.AddCRC(ref packet);
            AddCRC(ref packet);
            Byte[] packetCompare = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };
            Assert.AreEqual(8, packet.Length);
            Assert.AreEqual(packetCompare, packet);                        
        }
        [Test] 
        [ExpectedException(typeof(ArgumentNullException))]     
        public void AddCRC_ShouldThrowOnNullArgument()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet
            Byte[] packetNull = null;
            AddCRC(ref packetNull);                                   
        }
        [Test] 
        [ExpectedException(typeof(ArgumentOutOfRangeException))]     
        public void AddCRC_ShouldThrowOnGreaterThanMaxSizePacket()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //invalid length packet (exceeds max length)
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };
            Array.Resize<Byte>(ref packet, 255);
            AddCRC(ref packet);                            
        }
        [Test] 
        [ExpectedException(typeof(ArgumentOutOfRangeException))]     
        public void AddCRC_ShouldThrowOnLessThanMinSizePacket()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //invalid length packet (less than min length)
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };
            Array.Resize<Byte>(ref packet, 2);
            AddCRC(ref packet);                            
        }                              
        [Test]
        public void CheckCRC_ShouldWorkProperlyOnVariousTypesOfPacket()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet            
            Byte[] packet = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };
            bool bRetCode = CheckCRC(packet);            
            Assert.AreEqual(true, bRetCode);                      
            //null packet
            Byte[] packetNull = null;
            bRetCode = CheckCRC(packetNull);
            Assert.AreEqual(false, bRetCode);
            //invalid length packet (exceeds max length)
            Array.Resize<Byte>(ref packet, 270);
            bRetCode = CheckCRC(packet);
            Assert.AreEqual(false, bRetCode);           
            //invalid length packet (less than min length)
            Array.Resize<Byte>(ref packet, 2);
            bRetCode = CheckCRC(packet);
            Assert.AreEqual(false, bRetCode);           
        }
        [Test]
        public void CheckPacket_ShouldReturnOKCodeOnValidPacket()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            //valid packet                      
            Byte[] packetRecieve = {0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44};
            ModbusErrorCode err = CheckPacket(packetRecieve, 0x01, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeOK, err);
        }
        [Test]
        public void CheckPacket_ShouldReturncodeInvalidPacketLengthOnNullPacket()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();     
            //null packet            
            ModbusErrorCode err = CheckPacket(null, 0x01, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidPacketLength, err);
        }
        [Test]
        public void CheckPacket_ShouldReturncodeInvalidFunctionOnInvalidFuncCode()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();          
            Byte[] packetRecieve = {0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44};
            //invalid function code
            ModbusErrorCode err = CheckPacket(packetRecieve, 0x01, 0x02, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidFunction, err);
        }
        [Test]
        public void CheckPacket_ShouldReturncodeInvalidSlaveAddressOnInvalidSlaveAddress()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();   
            Byte[] packetRecieve = {0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44};
            //invalid slave address
            ModbusErrorCode err = CheckPacket(packetRecieve, 0x02, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidSlaveAddress, err);
        }
        [Test]
        public void CheckPacket_ShouldReturncodeInvalidPacketLengthInApropriateConditionsOnInvalidPacketLength()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRecieve = { 0x01, 0x03, 0x02, 0x00, 0x00, 0xB8, 0x44 };
            //invalid length packet
            Array.Resize<byte>(ref packetRecieve,6);
            ModbusErrorCode err = CheckPacket(packetRecieve, 0x01, 0x03, 7);
            Assert.AreEqual(ModbusErrorCode.codeInvalidPacketLength, err);
        }
        [Test]
        public void MakePacket_ShouldCreateModbusPacketOfApropriateLengthAndContents()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetSendCompare = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 };
            Byte[] packetSend = null;
            Byte slaveAddr = 1;
            Byte funcCode = 3;
            UInt16 startAddress = 100;
            UInt16 quantity = 1;
            packetSend = MakePacket(slaveAddr, funcCode, startAddress, quantity);
            Assert.AreEqual(packetSendCompare, packetSend);
        }
        [Test]
        public void ProcessData_ShouldProcessRawPacketBytesToApropriateValuesOfApropriateTypeIntoOutputArrayOfNumericObjectsAndReturnTrueOnSuccess_ReverseOrderOfRegisters()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();                                            

            Byte[] packetRawData = {   0xFE, //-2
                                       0x20, //32                                       
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
            //object[] arrayValues = { (Byte)0, (SByte)0, new ModbusDataRegisterUInt16(), (UInt16)0, (UInt32)0, new ModbusDataRegisterInt32(), (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
            object[] arrayValues = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
            ModbusDataMappingHelper.SwapBytesInWordsInByteArray(ref packetRawData);
            ProcessAnalogData(packetRawData, ref arrayValues, true);
            
            Assert.AreEqual(32, arrayValues[0]);
            Assert.AreEqual(-2, arrayValues[1]);
            Assert.AreEqual(-1, arrayValues[2]);
            Assert.AreEqual(500, arrayValues[3]);
            Assert.AreEqual(1234567890, arrayValues[4]);
            Assert.AreEqual(-123456789, arrayValues[5]);
            Assert.AreEqual(123.45f, (Single)arrayValues[6], 0.001f);
            Assert.AreEqual(1234567891011121314, arrayValues[7]);
            Assert.AreEqual(-1234567891011121314, arrayValues[8]);
            Assert.AreEqual(12345.67891, (Double)arrayValues[9]);
            Assert.AreEqual(1234567890.123456789m, arrayValues[10]);
        }
        [Test]
        public void ProcessData_ShouldProcessRawPacketBytesToApropriateValuesOfApropriateTypeIntoOutputArrayOfNumericObjectsAndReturnTrueOnSuccess_StraightOrderOfRegisters()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();

            Byte[] packetRawData = {   0xFE, //-2
                                       0x20, //32                                       
                                       0xFF, 0xFF, //-1
                                       0x01, 0xF4, //500                                    
                                       0x02, 0xD2, 0x49, 0x96,  // 1234567890
                                       0x32, 0xEB, 0xF8, 0xA4 , //-123456789
                                       0xE6, 0x66, 0x42, 0xF6,  // 123.45       
                                       0x30,0xA2,0xB2,0xD2,0x10,0xF4,0x11,0x22,//1234567891011121314
                                       0xCF,0x5E,0x4D,0x2D,0xEF,0x0B,0xEE,0xDD,//-1234567891011121314                                       
                                       0xdb,0x77,0xe6,0x85,0x1c,0xd6,0x40,0xc8,//12345.67891     
                                       0x81, 0x15, 0x7D, 0xE9, 0x10, 0xF4, 0x11, 0x22, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x9//1234567890.123456789
                                    };
            //object[] arrayValues = { (Byte)0, (SByte)0, new ModbusDataRegisterUInt16(), (UInt16)0, (UInt32)0, new ModbusDataRegisterInt32(), (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
            object[] arrayValues = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m };
            ModbusDataMappingHelper.SwapBytesInWordsInByteArray(ref packetRawData);
            ProcessAnalogData(packetRawData, ref arrayValues);

            Assert.AreEqual(32, arrayValues[0]);
            Assert.AreEqual(-2, arrayValues[1]);
            Assert.AreEqual(-1, arrayValues[2]);
            Assert.AreEqual(500, arrayValues[3]);
            Assert.AreEqual(1234567890, arrayValues[4]);
            Assert.AreEqual(-123456789, arrayValues[5]);
            Assert.AreEqual(123.45f, (Single)arrayValues[6], 0.001f);
            Assert.AreEqual(1234567891011121314, arrayValues[7]);
            Assert.AreEqual(-1234567891011121314, arrayValues[8]);
            Assert.AreEqual(12345.67891, (Double)arrayValues[9]);
            Assert.AreEqual(1234567890.123456789m, arrayValues[10]);
        }
        [Test]
        public void ProcessData_ShouldProcessRawPacketDataToApropriateValuesOfApropriateTypeIntoOutputArrayOfComplexObjectsAndReturnTrueOnSuccess_ReverseOrderOfRegisters()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();

            Byte[] packetRawData = {   0xFE, //-2
                                       0x20, //32                                      
                                       0xFF, 0xFF, //-1
                                       0x01, 0xF4, //500                                    
                                       0x49, 0x96, 0x02, 0xD2, //1234567890
                                       0xF8, 0xA4, 0x32, 0xEB, //-123456789
                                       0x42, 0xF6, 0xE6, 0x66, //123.45       
                                       0x11, 0x22, 0x10, 0xF4, 0xB2, 0xD2, 0x30, 0xA2,//1234567891011121314
                                       0xEE, 0xDD, 0xEF, 0x0B, 0x4D, 0x2D, 0xCF, 0x5E,//-1234567891011121314
                                       0x40, 0xc8, 0x1c, 0xd6, 0xe6, 0x85, 0xdb, 0x77,//12345.67891     
                                       0x0, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x11, 0x22, 0x10, 0xF4, 0x7D, 0xE9, 0x81, 0x15//1234567890.123456789
                                    };
            object[] arrayValues = { new ModbusDataPoint<Byte>(), 
                                     new ModbusDataPoint<SByte>(), 
                                     new ModbusDataPoint<Int16>(),                                      
                                     new ModbusDataPoint<UInt16>(), 
                                     new ModbusDataPoint<UInt32>(), 
                                     new ModbusDataPoint<Int32>(), 
                                     new ModbusDataPoint<Single>(), 
                                     new ModbusDataPoint<UInt64>(), 
                                     new ModbusDataPoint<Int64>(),  
                                     new ModbusDataPoint<Double>(), 
                                     new ModbusDataPoint<Decimal>()};

            ModbusDataMappingHelper.SwapBytesInWordsInByteArray(ref packetRawData);

            ProcessAnalogData(packetRawData, ref arrayValues, true);
            
            Assert.AreEqual(32, ((ModbusDataPoint<Byte>)arrayValues[0]).Value);
            Assert.AreEqual(-2, ((ModbusDataPoint<SByte>)arrayValues[1]).Value);
            Assert.AreEqual(-1, ((ModbusDataPoint<Int16>)arrayValues[2]).Value);
            Assert.AreEqual(500, ((ModbusDataPoint<UInt16>)arrayValues[3]).Value);
            Assert.AreEqual(1234567890, ((ModbusDataPoint<UInt32>)arrayValues[4]).Value);
            Assert.AreEqual(-123456789, ((ModbusDataPoint<Int32>)arrayValues[5]).Value);
            Assert.AreEqual(123.45f, ((ModbusDataPoint<Single>)arrayValues[6]).Value, 0.001f);
            Assert.AreEqual(1234567891011121314, ((ModbusDataPoint<UInt64>)arrayValues[7]).Value);
            Assert.AreEqual(-1234567891011121314, ((ModbusDataPoint<Int64>)arrayValues[8]).Value);
            Assert.AreEqual(12345.67891, ((ModbusDataPoint<Double>)arrayValues[9]).Value);
            Assert.AreEqual(1234567890.123456789m, ((ModbusDataPoint<Decimal>)arrayValues[10]).Value);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ProcessData_ShouldThrowIfNotValueTypeElementsAreRequestedInOutputArray()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                     0x01, 0xF4, //500
                                     0x00, 0x00, //0
                                     0x49, 0x96, 0x02, 0xD2, //1234567890
                                     };
            object[] rtuData = { (String)"aaa", new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new UInt32() };
            ProcessAnalogData(packetRawData, ref rtuData);                   
        }
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ProcessData_ShouldThrowIfAmmountOfInfoRequestedInOutputArrayExceedsCapacityOfRawDataArray()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                     0x01, 0xF4, //500
                                     0x00, 0x00, //0
                                     0x49, 0x96, 0x02, 0xD2, //1234567890
                                     };
            object[] rtuData = { new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new UInt32(), new UInt32(), new UInt32() };
            ProcessAnalogData(packetRawData, ref rtuData);           
        }
        [Test]
        public void ProcessData_ShouldNotThrowTrueIfAmmountOfInfoRequestedInOutputArrayLessThanCapacityOfRawDataArray()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                     0x01, 0xF4, //500
                                     0x00, 0x00, //0
                                     0x49, 0x96, 0x02, 0xD2, //1234567890
                                     };
            object[] rtuData = { new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>() };
            ProcessAnalogData(packetRawData, ref rtuData);            
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessData_ShouldThrowOnNullInputArgument()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            object[] rtuData = { new Int16(), new ModbusDataPoint<UInt16>(), new ModbusDataPoint<UInt16>(), new UInt32() };
            ProcessAnalogData(null, ref rtuData);     
        }
        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void ProcessData_ShouldThrowOnNullRefOutputArgument()
        {
            //ModbusRTUProtocol prot = new ModbusRTUProtocol();
            Byte[] packetRawData = { 0xFF, 0xFF, //-1
                                     0x01, 0xF4, //500
                                     0x00, 0x00, //0
                                     0x49, 0x96, 0x02, 0xD2, //1234567890
                                     };
            object[] rtuData = new object[2];
            ProcessAnalogData(packetRawData, ref rtuData);
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

        [Test]
        public void SizeOfPublicPropertiesOfClassArray_ShouldReturnValidSize()
        {
            object[] acl = { new MyClassWithPrivateProperty(), new MyClassWithPrivateProperty() };
            UInt32 size = SizeofHelper.SizeOfPublicProperties(acl);
            Assert.AreEqual(32, size);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SizeOfPublicPropertiesOfClassArray_ShouldThrowOnNullArgument()
        {
            UInt32 size = SizeofHelper.SizeOfPublicProperties(null);
        }
    }

    class ModbusDataMappingHelperTest
    {
        [Test]
        public void CreateArrayFromTypesArray_ShouldReturnArrayOfObjectsWithValidSizeAndValidTypesAndInTheSameOrder()
        {
            MyClass cl = new MyClass();
            Type[] arrayOfTypes = ModbusDataMappingHelper.GetObjectPropertiesTypeArray(cl);

            object[] arrayValues = ModbusDataMappingHelper.CreateValuesArrayFromTypesArray(arrayOfTypes);
            Assert.AreEqual(arrayOfTypes.Length,arrayValues.Length);

            for (int j = 0; j < arrayOfTypes.Length; j++)
            {
                Assert.AreEqual(arrayValues[j].GetType(), arrayOfTypes[j]);
            }           
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateArrayFromTypesArray_ShouldThrowOnNullArrayArgument()
        {
            object[] arrayValues = ModbusDataMappingHelper.CreateValuesArrayFromTypesArray(null);
        }
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
        public void ExtractValueFromArrayByType_ShouldSetApropriateValueFromInputArrayToOutputArgument_ReverseRegsOrder()
        {
            Byte[] packetRawData = {   0xFE, //-2
                                       0x20, //32                                       
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
            ModbusDataMappingHelper.SwapBytesInWordsInByteArray(ref packetRawData);
            for (int i = 0; i < arrayValues.Length; i++)
            {
                ModbusDataMappingHelper.ExtractValueFromArrayByType(packetRawData, ref packetCurrentPositionIndex, ref arrayValues[i],true);                 
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

        [Test]
        public void ExtractValueFromArrayByType_ShouldSetApropriateValueFromInputArrayToOutputArgument_StraightRegsOrder()
        {
            Byte[] packetRawData = {   0xFE, //-2
                                       0x20, //32                                       
                                       0xFF, 0xFF, //-1
                                       0x01, 0xF4, //500                                    
                                       0x02, 0xD2, 0x49, 0x96, //1234567890
                                       0x32, 0xEB, 0xF8, 0xA4 , //-123456789
                                       0xE6, 0x66, 0x42, 0xF6, //123.45       
                                       0x30,0xA2,0xB2,0xD2,0x10,0xF4,0x11,0x22,//1234567891011121314
                                       0xCF,0x5E,0x4D,0x2D,0xEF,0x0B,0xEE,0xDD,//-1234567891011121314                                       
                                       0xdb,0x77,0xe6,0x85,0x1c,0xd6,0x40,0xc8,//12345.67891     
                                       0x81, 0x15, 0x7D, 0xE9, 0x10, 0xF4, 0x11, 0x22, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x9//1234567890.123456789
                                    };
            object[] arrayValues = { (Byte)0, (SByte)0, (Int16)0, (UInt16)0, (UInt32)0, (Int32)0, (Single)0.0, (UInt64)0, (Int64)0, (Double)0.0, (Decimal)0m};
            int packetCurrentPositionIndex = 0;

            ModbusDataMappingHelper.SwapBytesInWordsInByteArray(ref packetRawData);
            for (int i = 0; i < arrayValues.Length; i++)
            {
                ModbusDataMappingHelper.ExtractValueFromArrayByType(packetRawData, ref packetCurrentPositionIndex, ref arrayValues[i], false);
            }
            Assert.AreEqual(32, arrayValues[0]);
            Assert.AreEqual(-2, arrayValues[1]);
            Assert.AreEqual(-1, arrayValues[2]);
            Assert.AreEqual(500, arrayValues[3]);
            Assert.AreEqual(1234567890, arrayValues[4]);
            Assert.AreEqual(-123456789, arrayValues[5]);
            Assert.AreEqual(123.45f, (Single)arrayValues[6], 0.001f);
            Assert.AreEqual(1234567891011121314, arrayValues[7]);
            Assert.AreEqual(-1234567891011121314, arrayValues[8]);
            Assert.AreEqual(12345.67891, (Double)arrayValues[9]);
            Assert.AreEqual(1234567890.123456789m, arrayValues[10]);
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertObjectValueAndAppendToArray_ShouldThrowOnNullArgument()
        {
            Byte[] arrBytes = new byte[5];
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes,null);            
        }        
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertObjectValueAndAppendToArray_ShouldThrowOnNotNumericTypeArgument()
        {
            Byte[] arrBytes = new byte[5];
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, "ssss");
        }
        [Test]        
        public void ConvertObjectValueAndAppendToArray_ShouldResizeArrayAndSetProperValues()
        {
            Byte[] arrBytes = null;
           
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, SByte.MinValue);
            Assert.AreEqual(1, arrBytes.Length);
            Assert.AreEqual(0x80, arrBytes[0]);
           
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Byte)(1));
            Assert.AreEqual(2, arrBytes.Length);
            Assert.AreEqual(0x01, arrBytes[1]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (UInt16)0xAA55);
            Assert.AreEqual(4, arrBytes.Length);
            Assert.AreEqual(0x55, arrBytes[2]);
            Assert.AreEqual(0xAA, arrBytes[3]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Int16)(-123));
            Assert.AreEqual(6, arrBytes.Length);
            Assert.AreEqual(0x85, arrBytes[4]);
            Assert.AreEqual(0xFF, arrBytes[5]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (UInt32)(123456789));
            Assert.AreEqual(10, arrBytes.Length);
            Assert.AreEqual(0x15, arrBytes[6]);
            Assert.AreEqual(0xCD, arrBytes[7]);
            Assert.AreEqual(0x5B, arrBytes[8]);
            Assert.AreEqual(0x07, arrBytes[9]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Int32)(-123456789));
            Assert.AreEqual(14, arrBytes.Length);
            Assert.AreEqual(0xEB, arrBytes[10]);
            Assert.AreEqual(0x32, arrBytes[11]);
            Assert.AreEqual(0xA4, arrBytes[12]);
            Assert.AreEqual(0xF8, arrBytes[13]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (UInt64)(1234567891011121314));
            Assert.AreEqual(22, arrBytes.Length);
            Assert.AreEqual(0xA2, arrBytes[14]);
            Assert.AreEqual(0x30, arrBytes[15]);
            Assert.AreEqual(0xD2, arrBytes[16]);
            Assert.AreEqual(0xB2, arrBytes[17]);
            Assert.AreEqual(0xF4, arrBytes[18]);
            Assert.AreEqual(0x10, arrBytes[19]);
            Assert.AreEqual(0x22, arrBytes[20]);
            Assert.AreEqual(0x11, arrBytes[21]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Int64)(-1234567891011121314));
            Assert.AreEqual(30, arrBytes.Length);
            Assert.AreEqual(0x5E, arrBytes[22]);
            Assert.AreEqual(0xCF, arrBytes[23]);
            Assert.AreEqual(0x2D, arrBytes[24]);
            Assert.AreEqual(0x4D, arrBytes[25]);
            Assert.AreEqual(0x0B, arrBytes[26]);
            Assert.AreEqual(0xEF, arrBytes[27]);
            Assert.AreEqual(0xDD, arrBytes[28]);
            Assert.AreEqual(0xEE, arrBytes[29]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Single)(123.45f));
            Assert.AreEqual(34, arrBytes.Length);
            Assert.AreEqual(0x66, arrBytes[30]);
            Assert.AreEqual(0xE6, arrBytes[31]);
            Assert.AreEqual(0xF6, arrBytes[32]);
            Assert.AreEqual(0x42, arrBytes[33]);
           
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Double)(12345.67891));
            Assert.AreEqual(42, arrBytes.Length);
            Assert.AreEqual(0x77, arrBytes[34]);
            Assert.AreEqual(0xDB, arrBytes[35]);
            Assert.AreEqual(0x85, arrBytes[36]);
            Assert.AreEqual(0xE6, arrBytes[37]);
            Assert.AreEqual(0xD6, arrBytes[38]);
            Assert.AreEqual(0x1C, arrBytes[39]);
            Assert.AreEqual(0xC8, arrBytes[40]);
            Assert.AreEqual(0x40, arrBytes[41]);
            
            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, 1234567890.123456789m);
            Assert.AreEqual(58, arrBytes.Length);
            Assert.AreEqual(0x15, arrBytes[42]);
            Assert.AreEqual(0x81, arrBytes[43]);
            Assert.AreEqual(0xE9, arrBytes[44]);
            Assert.AreEqual(0x7D, arrBytes[45]);
            Assert.AreEqual(0xF4, arrBytes[46]);
            Assert.AreEqual(0x10, arrBytes[47]);
            Assert.AreEqual(0x22, arrBytes[48]);
            Assert.AreEqual(0x11, arrBytes[49]);
            Assert.AreEqual(0x00, arrBytes[50]);
            Assert.AreEqual(0x00, arrBytes[51]);
            Assert.AreEqual(0x00, arrBytes[52]);
            Assert.AreEqual(0x00, arrBytes[53]);
            Assert.AreEqual(0x00, arrBytes[54]);
            Assert.AreEqual(0x00, arrBytes[55]);
            Assert.AreEqual(0x09, arrBytes[56]);
            Assert.AreEqual(0x00, arrBytes[57]);
        }
        [Test]
        public void ConvertObjectValueAndAppendToArray_ShouldResizeArrayAndSetProperValuesInBigEndianOrder()
        {
            Byte[] arrBytes = null;

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (SByte)(-1),true);
            Assert.AreEqual(1, arrBytes.Length);
            Assert.AreEqual(0xFF, arrBytes[0]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Byte)(1), true);
            Assert.AreEqual(2, arrBytes.Length);
            Assert.AreEqual(0x01, arrBytes[1]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (UInt16)0xAA55, true);
            Assert.AreEqual(4, arrBytes.Length);
            Assert.AreEqual(0x55, arrBytes[2]);
            Assert.AreEqual(0xAA, arrBytes[3]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Int16)(-123), true);
            Assert.AreEqual(6, arrBytes.Length);
            Assert.AreEqual(0x85, arrBytes[4]);
            Assert.AreEqual(0xFF, arrBytes[5]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (UInt32)(123456789), true);
            Assert.AreEqual(10, arrBytes.Length);
            Assert.AreEqual(0x5B, arrBytes[6]);
            Assert.AreEqual(0x07, arrBytes[7]);
            Assert.AreEqual(0x15, arrBytes[8]);
            Assert.AreEqual(0xCD, arrBytes[9]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Int32)(-123456789), true);
            Assert.AreEqual(14, arrBytes.Length);
            Assert.AreEqual(0xA4, arrBytes[10]);
            Assert.AreEqual(0xF8, arrBytes[11]);
            Assert.AreEqual(0xEB, arrBytes[12]);
            Assert.AreEqual(0x32, arrBytes[13]);            

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (UInt64)(1234567891011121314), true);
            Assert.AreEqual(22, arrBytes.Length);
            Assert.AreEqual(0xA2, arrBytes[20]);
            Assert.AreEqual(0x30, arrBytes[21]);
            Assert.AreEqual(0xD2, arrBytes[18]);
            Assert.AreEqual(0xB2, arrBytes[19]);
            Assert.AreEqual(0xF4, arrBytes[16]);
            Assert.AreEqual(0x10, arrBytes[17]);
            Assert.AreEqual(0x22, arrBytes[14]);
            Assert.AreEqual(0x11, arrBytes[15]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Int64)(-1234567891011121314), true);
            Assert.AreEqual(30, arrBytes.Length);
            Assert.AreEqual(0x5E, arrBytes[28]);
            Assert.AreEqual(0xCF, arrBytes[29]);
            Assert.AreEqual(0x2D, arrBytes[26]);
            Assert.AreEqual(0x4D, arrBytes[27]);
            Assert.AreEqual(0x0B, arrBytes[24]);
            Assert.AreEqual(0xEF, arrBytes[25]);
            Assert.AreEqual(0xDD, arrBytes[22]);
            Assert.AreEqual(0xEE, arrBytes[23]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Single)(123.45f), true);
            Assert.AreEqual(34, arrBytes.Length);
            Assert.AreEqual(0x66, arrBytes[32]);
            Assert.AreEqual(0xE6, arrBytes[33]);
            Assert.AreEqual(0xF6, arrBytes[30]);
            Assert.AreEqual(0x42, arrBytes[31]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, (Double)(12345.67891), true);
            Assert.AreEqual(42, arrBytes.Length);
            Assert.AreEqual(0x77, arrBytes[40]);
            Assert.AreEqual(0xDB, arrBytes[41]);
            Assert.AreEqual(0x85, arrBytes[38]);
            Assert.AreEqual(0xE6, arrBytes[39]);
            Assert.AreEqual(0xD6, arrBytes[36]);
            Assert.AreEqual(0x1C, arrBytes[37]);
            Assert.AreEqual(0xC8, arrBytes[34]);
            Assert.AreEqual(0x40, arrBytes[35]);

            ModbusDataMappingHelper.ConvertObjectValueAndAppendToArray(ref arrBytes, 1234567890.123456789m, true);
            Assert.AreEqual(58, arrBytes.Length);
            Assert.AreEqual(0x15, arrBytes[56]);
            Assert.AreEqual(0x81, arrBytes[57]);
            Assert.AreEqual(0xE9, arrBytes[54]);
            Assert.AreEqual(0x7D, arrBytes[55]);
            Assert.AreEqual(0xF4, arrBytes[52]);
            Assert.AreEqual(0x10, arrBytes[53]);
            Assert.AreEqual(0x22, arrBytes[50]);
            Assert.AreEqual(0x11, arrBytes[51]);
            Assert.AreEqual(0x00, arrBytes[48]);
            Assert.AreEqual(0x00, arrBytes[49]);
            Assert.AreEqual(0x00, arrBytes[46]);
            Assert.AreEqual(0x00, arrBytes[47]);
            Assert.AreEqual(0x00, arrBytes[44]);
            Assert.AreEqual(0x00, arrBytes[45]);
            Assert.AreEqual(0x09, arrBytes[42]);
            Assert.AreEqual(0x00, arrBytes[43]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReverseWordsInBlocksOfByteArray_ShouldThrowOnNullArgument()
        {
            Byte[] arr = null;
            ModbusDataMappingHelper.ReverseWordsInBlocksOfByteArray(ref arr, 0, 2, 1);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReverseWordsInBlocksOfByteArray_ShouldThrowOnNonDivisibleBySeedArgumentArrayLength()
        {
            Byte[] arr = { 33, 44, 55, 66, 77 };
            ModbusDataMappingHelper.ReverseWordsInBlocksOfByteArray(ref arr, 0, 4, 3);
        }
        [Test]
        public void ReverseWordsInBlocksOfByteArray_ShouldSwapWordsInByteArray()
        {           
            Byte[] arr2 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            ModbusDataMappingHelper.ReverseWordsInBlocksOfByteArray(ref arr2, 1, 16, 2);
            Assert.AreEqual(15, arr2[1]);
            Assert.AreEqual(16, arr2[2]);
            Assert.AreEqual(13, arr2[3]);
            Assert.AreEqual(14, arr2[4]);
            Assert.AreEqual(11, arr2[5]);
            Assert.AreEqual(12, arr2[6]);
            Assert.AreEqual(9,  arr2[7]);
            Assert.AreEqual(10, arr2[8]);
            Assert.AreEqual(7, arr2[9]);
            Assert.AreEqual(8, arr2[10]);
            Assert.AreEqual(5, arr2[11]);
            Assert.AreEqual(6, arr2[12]);
            Assert.AreEqual(3, arr2[13]);
            Assert.AreEqual(4, arr2[14]);
            Assert.AreEqual(1, arr2[15]);
            Assert.AreEqual(2, arr2[16]);
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
    /*class ModbusLoggerTest
    {
        [Test]
        public void CtorShouldSetDefaultValues()
        {
            ModbusLogger log = new ModbusLogger();
            Assert.AreEqual(@"${basedir}\", log.ExceptionLogDir);            
        }

        [Test]
        public void ExceptionLogDir_ShouldChangeExceptionLogFilePath()
        {
            ModbusLogger log = new ModbusLogger();
            log.ExceptionLogDir = @"d:\";
            Assert.AreEqual(@"d:\ModbusCoreExceptions.txt", log.ExceptionLogFilePath);
        }
        [Test]
        public void ShouldSaveExceptionsToProperFile()
        {
            ModbusLogger log = new ModbusLogger();            
                        
            ArgumentNullException exception = new ArgumentNullException();
                            
            log.ExceptionLogDir = @"d:\aaaa\";            
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
    }*/
}
