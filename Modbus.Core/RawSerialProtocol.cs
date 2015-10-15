using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Text.RegularExpressions;
using System.IO;

namespace Modbus.Core
{
    public class RawSerialProtocol
    {
        #region Properties        
        public bool IsConnected { get; private set; }
        public String StatusString { get; private set; }
        public SerialPortPin TangentaPin { get; set; }
        public bool SaveExceptionsToLog { get; set; }                
        public String LogsPath 
        {
            get
            {
                return logger.ExceptionLogDir;
            }
            set
            {              
                Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
                if (!driveCheck.IsMatch(value.Substring(0, 3)))
                {
                    throw new ArgumentException();                    
                }
                string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
                strTheseAreInvalidFileNameChars += @":/?*" + "\"";
                Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
                if (containsABadCharacter.IsMatch(value.Substring(3, value.Length - 3)))
                {
                    throw new ArgumentException();                   
                }

                DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(value));
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException();                    
                }               
                logger.ExceptionLogDir = value;
            }
        }
        public int SilentInterval 
        { 
            get 
            {
                return silentInterval; 
            } 
            set 
            {
                if (value < 1)
                {
                    silentInterval = 1;
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    throw (ex);
                }
                else
                    silentInterval = value;
            } 
        }       
        public int TangentaSetPinTimePeriodMsec
        {
            get
            {
                return tangentaSetPinTimePeriodMsec;
            }
            set
            {
                if (value < 0)
                {
                    tangentaSetPinTimePeriodMsec = 0;
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    throw (ex);
                }
                else
                    tangentaSetPinTimePeriodMsec = value;
            }
        }
        public int Retries
        {
            get { return retries; }
            set
            {
                if (value < 1)
                {
                    retries = 1;
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    throw (ex);
                }
                else
                    retries = value;
            }
        }
        #endregion
               
        #region Variables
        private int retries;
        private SerialPort comPort;
        private int silentInterval;
        private int tangentaSetPinTimePeriodMsec;
        private ModbusLogger logger;
        #endregion

        #region Ctors
        public RawSerialProtocol()
        {
            comPort = new SerialPort();           
            IsConnected = false;
            StatusString = "Not connected";
            SilentInterval = 20;
            TangentaPin = SerialPortPin.None;
            TangentaSetPinTimePeriodMsec = 100;
            Retries = 1;
            SaveExceptionsToLog = false;            
            logger = new ModbusLogger();                        
            logger.ExceptionLogDir = AppDomain.CurrentDomain.BaseDirectory;
        }
        #endregion

        #region Connect/Disconnect procedures
        public bool Connect(string portName, int baudRate=9600, int byteSize = 8, StopBits stopBits=StopBits.Two, Parity parity=Parity.None, int timeout=1000,Handshake handShake=Handshake.None)
        {
            if (comPort.IsOpen)
                comPort.Close();               
            try
            {
                comPort.PortName = portName;
                comPort.BaudRate = baudRate;
                comPort.DataBits = byteSize;
                comPort.StopBits = stopBits;
                comPort.Parity = parity;
                comPort.ReadTimeout = timeout;        
                comPort.WriteTimeout = 1000;
                comPort.Handshake = handShake;                
                comPort.Open();
            }
            catch (Exception error)
            {
                StatusString = "Error in Connect: " + error.Message;
                if (SaveExceptionsToLog)
                    logger.SaveException(error);
                return false;
            }
            StatusString = "Connected";
            IsConnected = comPort.IsOpen;
            return comPort.IsOpen;
        }
        public void Disconnect()
        {           
            if (comPort.IsOpen)            
                comPort.Close();
            IsConnected = comPort.IsOpen;                
            StatusString = "Not connected";
        }
        #endregion

        #region Send/Recieve operations     
  
        public bool RecivePacket(ref byte[] packet)
        {
            if (comPort.IsOpen == false)
            {
                StatusString = "Error in RecivePacket: port not opened";
                return false;
            }
            try
            {
                int bytesTotallyRecieved = 0;               
                bool smthRead = false;
                int sleepTime = (SilentInterval / 3 == 0) ? 1 : SilentInterval / 3;
                int stepsCount = comPort.ReadTimeout / sleepTime;
                int silenceTime = 0;

                for (int i = 0; i < stepsCount; i++)
                {
                    int bytesRecieved = comPort.BytesToRead;
                    if (bytesRecieved > 0)
                    {
                        silenceTime = 0;
                        smthRead = true;
                        Array.Resize<Byte>(ref packet, bytesTotallyRecieved + bytesRecieved);
                        comPort.Read(packet, bytesTotallyRecieved, bytesRecieved);
                        bytesTotallyRecieved += bytesRecieved;                        
                    }
                    else 
                    {
                        silenceTime += sleepTime;
                        if (smthRead && silenceTime > SilentInterval)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(sleepTime);
                }
                if (smthRead == false)
                {
                    TimeoutException ex = new TimeoutException();
                    throw(ex);
                }
                                
                StatusString = "OK";
                return true;
            }
            catch (TimeoutException)
            {
                throw;               
            }
            catch (Exception error)
            {
                if (SaveExceptionsToLog)
                    logger.SaveException(error);
                return false;
            }            
        }
        

        public bool SendPacket(byte[] packet)
        {            
            if (comPort.IsOpen == false)
            {
                StatusString = "Error in SendPacket: port not opened";
                return false;
            }

            try
            {
                if (comPort.Handshake == Handshake.None)
                {
                    switch (TangentaPin)
                    {
                        case SerialPortPin.RTS:
                            {
                                comPort.RtsEnable = true;
                                Thread.Sleep(tangentaSetPinTimePeriodMsec);
                                break;
                            }
                        case SerialPortPin.DTR:
                            {
                                comPort.DtrEnable = true;
                                Thread.Sleep(tangentaSetPinTimePeriodMsec);
                                break;
                            }
                    }
                }
                Thread.Sleep(silentInterval);
                comPort.Write(packet, 0, packet.Length);

                if (comPort.Handshake == Handshake.None)
                {
                    switch (TangentaPin)
                    {
                        case SerialPortPin.RTS:
                            comPort.RtsEnable = false;
                            break;

                        case SerialPortPin.DTR:
                            comPort.DtrEnable = false;
                            break;
                    }
                }
            }
            catch (Exception error)
            {
                StatusString = "Error in SendPacket: " + error.Message;
                if (SaveExceptionsToLog)
                    logger.SaveException(error);
                return false;
            }
            StatusString = "OK";
            return true;
        }

        public bool TxRxMessage(byte[] packetToSend, ref byte[] recievedPacket, ushort expectedRecievedPacketSize)
        {
            Array.Resize<byte>(ref recievedPacket, 0);

            for (int retry = 0; retry < Retries; retry++)
            {
                if (!SendPacket(packetToSend))
                    return false;
                try
                {
                    if (RecivePacket(ref recievedPacket))
                    {
                        if (expectedRecievedPacketSize == recievedPacket.Length)
                        {
                            if (Retries > 1)
                                StatusString = String.Format("OK (Retry:{0})", retry + 1);
                            return true;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    if (retry + 1 == Retries)
                    {
                        StatusString = "Error: Timeout";
                        if (Retries > 1)
                            StatusString = String.Format("Error: Timeout (Retries:{0})", retry + 1);
                        return false;
                    }
                }              
            }
            StatusString = "Error: ???";
            return false;
        }
        #endregion                 
    }
}