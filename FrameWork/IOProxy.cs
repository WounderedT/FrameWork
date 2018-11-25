using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWork
{
    public static class IOProxy
    {
        private static String _workDir;

        public static String WorkDirectory
        {
            get
            {
                if (String.IsNullOrEmpty(_workDir))
                {
                    var location = Assembly.GetCallingAssembly().Location;
                    _workDir = Path.GetDirectoryName(location);
                }
                return _workDir;
            }
        }

        public static String ErrorLogFilePath
        {
            get
            {
                var logDirPath = Path.Combine(WorkDirectory, "logs");
                Directory.CreateDirectory(logDirPath);
                return Path.Combine(logDirPath, DateTime.Now.ToString("ddMMyyyy-HHmmss", System.Globalization.CultureInfo.InvariantCulture) + ".log");
            }
        }

        public static String PluginsDirectory
        {
            get { return Path.Combine(WorkDirectory, "plugins"); }
        }

        public static MemoryStream GetMemoryStreamFromFile(string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            if (!File.Exists(filePath))
                return null;
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                fs.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }

        public static async Task<MemoryStream> GetMemoryStreamFromFileAsync(string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            if (!File.Exists(filePath))
                return null;
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                await fs.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            return ms;
        }

        public static string WriteMemoryStreamToFile(MemoryStream mStream, string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                mStream.WriteTo(fs);
            return filePath;
        }

        public static async Task WriteMemoryStreamToFileAsync(MemoryStream mStream, string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            byte[] bytes = mStream.ToArray();
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public static async Task<string> WriteBytesToFileAsync(byte[] array, string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                await fs.WriteAsync(array, 0, array.Length).ConfigureAwait(false);
            return filePath;
        }

        public static bool WritePassword(EncryptedPassword password, string filename)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, password);
            return EncryptFile(WriteMemoryStreamToFile(ms, filename));
        }

        public static bool Exists(string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            return File.Exists(filePath);
        }
        
        public static string FileNameToCode(string filename)
        {
            byte[] bytes = new byte[filename.Length * sizeof(char)];
            Buffer.BlockCopy(filename.ToCharArray(), 0, bytes, 0, bytes.Length);
            bytes = bytes.Where(w => w != 0).ToArray();
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        public static string RelativeToAbsolute(string filePath)
        {
            if (!Path.IsPathRooted(filePath)) //could be replaced by Path.IsPathRooted
            {
                String fileName = FileNameToCode(Path.GetFileName(filePath));
                String absoluteDir = Path.Combine(WorkDirectory, Path.GetDirectoryName(filePath));
                Directory.CreateDirectory(Path.GetDirectoryName(absoluteDir));
                return Path.Combine(absoluteDir, fileName);
            }
            else
                return filePath;
        }

        private static bool EncryptFile(string filepath)
        {
            if (!Settings.EncryptFiles)
                return true;
            try
            {
                File.Encrypt(filepath);
                return true;
            }
            catch (IOException ex)
            {
                string messageBoxText = ex.Message + Environment.NewLine 
                    + "You can proceed with the executin but your files will be less secure. "
                    +"You can disable file encryption in Settings if this error cannot be fixed."+Environment.NewLine + "Do you wish to continue?";
                string caption = "File encryption has failed!";
                var result = MessageBox.Show(Application.Current.MainWindow, messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                return result.Equals(MessageBoxResult.Yes) ? true : false;
            } 
        }
    }
}
