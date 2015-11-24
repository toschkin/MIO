using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using MIOConfig.InternalLayer;

namespace MIOConfig
{           
    public class FileReaderSaver : IDeviceReaderSaver
    {
        /// <summary>
        /// Gets whether the specified path is a valid absolute file path.
        /// </summary>
        /// <param name="path">Any path. OK if null or empty.</param>
        public static bool IsValidPath(string path)
        {
            //Simple pattern
            //@"^(([a-zA-Z]:)|(\))(\{1}|((\{1})[^\]([^/:*?<>""|]*))+)$"
            string DmitriyBorysovPattern =
                @"^(([a-zA-Z]:|\\)\\)?(((\.)|(\.\.)|([^\\/:\*\?\|<>\. ](([^\\/:\*\?\|<>\. ])|([^\\/:\*\?\|<>]*[^\\/:\*\?\|<>\. ]))?))\\)*[^\\/:\*\?\|<>\. ](([^\\/:\*\?\|<>\. ])|([^\\/:\*\?\|<>]*[^\\/:\*\?\|<>\. ]))?$";
            Regex r = new Regex(DmitriyBorysovPattern);
                
            return r.IsMatch(path);
        }

        public FileReaderSaver()
        {
            FilePath = @"MIOdevice.dat";
        }

        public FileReaderSaver(String path)
        {
            FilePath = path;
        }

        private String _filePath;
        public String FilePath {
            get { return _filePath; }
            set
            {
                if(!IsValidPath(value))
                    _filePath = @"MIOdevice.dat";
                _filePath = value;
            }
        }

        public ReaderSaverErrors SaveDeviceConfiguration(Device configuration)
        {
            BinaryFormatter formatter = new BinaryFormatter();
           
            try
            {
                FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate);
                formatter.Serialize(fs, configuration.Configuration);   
                fs.Close();
            }
            catch (Exception)
            {
                return ReaderSaverErrors.CodeSerializationError;
            }                            
            return ReaderSaverErrors.CodeOk;
        }

        public ReaderSaverErrors ReadDeviceConfiguration(ref Device configuration)
        {            
            BinaryFormatter formatter = new BinaryFormatter();
                       
            try
            {
                FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate);
                configuration.Configuration = (DeviceConfiguration)formatter.Deserialize(fs);                
                fs.Close();
            }
            catch (Exception)
            {
                return ReaderSaverErrors.CodeSerializationError;
            }
            return ReaderSaverErrors.CodeOk;
        }
    }
}
