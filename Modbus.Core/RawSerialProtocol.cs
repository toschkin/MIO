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
        //exceptions saving        
        protected event SaveException LogExceptionRsp;       

        #region Properties        
        public bool IsConnected { get; private set; }
        public String StatusString { get; private set; }
        public SerialPortPin TangentaPin { get; set; }
        //public bool SaveExceptionsToLog { get; set; }                
        /*public String ExceptionLogsPath 
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
        }*/
        public int SilentInterval 
        { 
            get 
            {
                return _silentInterval; 
            } 
            set 
            {
                if (value < 1)
                {
                    _silentInterval = 1;
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    throw (ex);
                }                
                _silentInterval = value;
            } 
        }       
        public int TangentaSetPinTimePeriodMsec
        {
            get
            {
                return _tangentaSetPinTimePeriodMsec;
            }
            set
            {
                if (value < 0)
                {
                    _tangentaSetPinTimePeriodMsec = 0;
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    throw (ex);
                }                
                _tangentaSetPinTimePeriodMsec = value;
            }
        }
        public int Retries
        {
            get { return _retries; }
            set
            {
                if (value < 1)
                {
                    _retries = 1;
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    throw (ex);
                }
                _retries = value;
            }
        }
        #endregion
               
        #region Variables
        private int _retries;
        private SerialPort _comPort;
        private int _silentInterval;
        private int _tangentaSetPinTimePeriodMsec;
        //protected ModbusLogger logger;
        #endregion

        #region Ctors
        public RawSerialProtocol()
        {
            _comPort = new SerialPort();           
            IsConnected = false;
            StatusString = "Not connected";
            SilentInterval = 20;
            TangentaPin = SerialPortPin.None;
            TangentaSetPinTimePeriodMsec = 100;
            Retries = 1;
            //SaveExceptionsToLog = false;            
            //logger = new ModbusLogger();                        
            //logger.ExceptionLogDir = AppDomain.CurrentDomain.BaseDirectory;            
        }
        #endregion

        #region Connect/Disconnect procedures

        public string GetConnectionParametersString()
        {
            return String.Format("{0} {1} {2}{3}{4}", _comPort.PortName, _comPort.BaudRate, _comPort.DataBits, _comPort.Parity.ToString()[0],(int)_comPort.StopBits);
        }

        public bool Connect(string portName, int baudRate=9600, int byteSize = 8, StopBits stopBits=StopBits.Two, Parity parity=Parity.None, int timeout=1000,Handshake handShake=Handshake.None)
        {
            if (_comPort.IsOpen)
                _comPort.Close();               
            try
            {
                _comPort.PortName = portName;
                _comPort.BaudRate = baudRate;
                _comPort.DataBits = byteSize;
                _comPort.StopBits = stopBits;
                _comPort.Parity = parity;
                _comPort.ReadTimeout = timeout;        
                _comPort.WriteTimeout = 1000;
                _comPort.Handshake = handShake;                
                _comPort.Open();
            }
            catch (Exception error)
            {
                StatusString = "Error in Connect: " + error.Message;
                if (LogExceptionRsp != null)
                    LogExceptionRsp(error);
                return false;
            }
            StatusString = "Connected";
            IsConnected = _comPort.IsOpen;
            return _comPort.IsOpen;
        }
        public void Disconnect()
        {           
            if (_comPort.IsOpen)            
                _comPort.Close();
            IsConnected = _comPort.IsOpen;                
            StatusString = "Not connected";
        }
        #endregion

        #region Send/Recieve operations     
  
        public bool RecivePacket(ref byte[] packet)
        {
            if (_comPort.IsOpen == false)
            {
                StatusString = "Error in RecivePacket: port not opened";
                return false;
            }
            try
            {
                int bytesTotallyRecieved = 0;               
                bool smthRead = false;
                int sleepTime = (SilentInterval / 3 == 0) ? 1 : SilentInterval / 3;
                int stepsCount = _comPort.ReadTimeout / sleepTime;
                int silenceTime = 0;

                for (int i = 0; i < stepsCount; i++)
                {
                    int bytesRecieved = _comPort.BytesToRead;
                    if (bytesRecieved > 0)
                    {
                        silenceTime = 0;
                        smthRead = true;
                        Array.Resize<Byte>(ref packet, bytesTotallyRecieved + bytesRecieved);
                        _comPort.Read(packet, bytesTotallyRecieved, bytesRecieved);
                        bytesTotallyRecieved += bytesRecieved;
                        continue;
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
                if (LogExceptionRsp != null)
                    LogExceptionRsp(error);
                return false;
            }            
        }
        
        public bool SendPacket(byte[] packet)
        {            
            if (_comPort.IsOpen == false)
            {
                StatusString = "Error in SendPacket: port not opened";
                return false;
            }

            try
            {
                if (_comPort.Handshake == Handshake.None)
                {
                    switch (TangentaPin)
                    {
                        case SerialPortPin.Rts:
                            {
                                _comPort.RtsEnable = true;
                                Thread.Sleep(_tangentaSetPinTimePeriodMsec);
                                break;
                            }
                        case SerialPortPin.Dtr:
                            {
                                _comPort.DtrEnable = true;
                                Thread.Sleep(_tangentaSetPinTimePeriodMsec);
                                break;
                            }
                    }
                }
                Thread.Sleep(_silentInterval);
                _comPort.Write(packet, 0, packet.Length);

                if (_comPort.Handshake == Handshake.None)
                {
                    switch (TangentaPin)
                    {
                        case SerialPortPin.Rts:
                            _comPort.RtsEnable = false;
                            break;

                        case SerialPortPin.Dtr:
                            _comPort.DtrEnable = false;
                            break;
                    }
                }
            }
            catch (Exception error)
            {
                StatusString = "Error in SendPacket: " + error.Message;
                if (LogExceptionRsp != null)
                    LogExceptionRsp(error);
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
                    if ((retry + 1 == Retries)&&(recievedPacket.Length == 0))
                    {
                        StatusString = "Error: Timeout";
                        if (Retries > 1)
                            StatusString = String.Format("Error: Timeout (Retries:{0})", retry + 1);
                        return false;
                    }
                }              
            }            
            StatusString = "Error: Invalid response length";
            return false;
        }
        #endregion                 
    }
}