using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;


namespace Modbus.Core
{  
    public class  ModbusLogger
    {
        #region Variables
        private string exceptionLogFilePath;
        private Logger logger;
        #endregion

        #region Properties
        public String ExceptionLogFilePath { get { return exceptionLogFilePath; } private set { exceptionLogFilePath = value; } }
        public String ExceptionLogDir
        {
            get
            {
                int index = exceptionLogFilePath.LastIndexOf('\\');
                if (index >= 0)
                    return exceptionLogFilePath.Remove(index + 1, exceptionLogFilePath.Length - index - 1);
                else
                    return "";
            }
            set
            {
                String temp = value;
                char[] slashes = { '\\', '/' };
                temp = temp.TrimEnd(slashes);
                temp += '\\';
                exceptionLogFilePath = temp + "ModbusCoreExceptions.txt";

                LoggingConfiguration config = LogManager.Configuration;
                config.RemoveTarget("ExceptionsFile");
                FileTarget fileTarget = new FileTarget();
                config.AddTarget("ExceptionsFile", fileTarget);
                // Step 3. Set target properties             
                fileTarget.FileName = exceptionLogFilePath;
                fileTarget.Layout = "${longdate} ${message} ${exception:format=ToString}";
                // Step 4. Define rules            
                LoggingRule rule = new LoggingRule("*", LogLevel.Error, fileTarget);
                config.LoggingRules.Add(rule);
                // Step 5. Activate the configuration
                LogManager.Configuration = config;
                // Example usage                
                logger = LogManager.GetLogger("ModbusLogger");
            }
        }
        #endregion

        #region Ctors
        public ModbusLogger()
        {
            exceptionLogFilePath = @"${basedir}\ModbusCoreExceptions.txt";
            // Step 1. Create configuration object 
            LoggingConfiguration config = new LoggingConfiguration();
            // Step 2. Create targets and add them to the configuration            
            FileTarget fileTarget = new FileTarget();
            config.AddTarget("ExceptionsFile", fileTarget);
            // Step 3. Set target properties             
            fileTarget.FileName = exceptionLogFilePath;
            fileTarget.Layout = "${longdate} ${message} ${exception:format=Method,ToString}";
            // Step 4. Define rules            
            LoggingRule rule = new LoggingRule("*", LogLevel.Error, fileTarget);
            config.LoggingRules.Add(rule);
            // Step 5. Activate the configuration
            LogManager.Configuration = config;
            // Example usage
            LogManager.ThrowExceptions = true;
            logger = LogManager.GetLogger("ModbusLogger");

        }
        #endregion

        #region Methods        
        public void SaveException(Exception exception)
        {
            logger.Error(exception);
        }
        #endregion
        
    }
}
