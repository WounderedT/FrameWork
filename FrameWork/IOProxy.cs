﻿using Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWork
{
    /*
        An unhandled exception of type 'System.IO.IOException' could be risen by File.Encrypt during
        password file encryption. Reason:
        Additional information: Recovery policy configured for this system contains invalid recovery certificate.
        Encryption could be skipped, but message with notification should be presented.(Based on user decision?)
    */
    public static class IOProxy
    {
        public static MemoryStream GetMemoryStreamFromFile(string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            if (!File.Exists(filePath))
            {
                return null;
            }
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                fs.CopyTo(ms);
            }
            ms.Position = 0;
            return ms;
        }

        public static async Task<MemoryStream> GetMemoryStreamFromFileAsync(string filename)
        {
            return await Task.Run(() => GetMemoryStreamFromFile(filename));
        }

        public static string WriteMemoryStreamToFile(MemoryStream mStream, string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                mStream.WriteTo(fs);
            }
            //File.Encrypt(filePath);
            return filePath;
        }

        public static async Task<string> WriteBytesToFileAsync(byte[] array, string filename)
        {
            string filePath = RelativeToAbsolute(filename);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fs.WriteAsync(array, 0, array.Length);
            }
            return filePath;
        }

        public static async Task WriteMemoryStreamToFileAsync(MemoryStream mStream, string filename)
        {
            await Task.Run(() => WriteMemoryStreamToFile(mStream, filename));
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
        
        private static string RelativeToAbsolute(string filePath)
        {
            if (!filePath.Contains(@":\")) //could be replaced by Path.IsPathRooted
            {
                string[] subpath = filePath.Split('\\');
                string fileName = FileNameToCode(subpath[subpath.Length - 1]);
                if (subpath.Length > 1)
                    subpath = subpath.Where(w => w != string.Empty && w != subpath[subpath.Length - 1]).ToArray();
                else
                    subpath[0] = string.Empty;
                filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(subpath));
                Directory.CreateDirectory(filePath);
                return Path.Combine(filePath, fileName);
            }
            else
            {
                return filePath;
            }
        }

        private static string FileNameToCode(string filename)
        {
            byte[] bytes = new byte[filename.Length * sizeof(char)];
            Buffer.BlockCopy(filename.ToCharArray(), 0, bytes, 0, bytes.Length);
            bytes = bytes.Where(w => w != 0).ToArray();
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        public static string GetCurrentProjectFolder()
        {
            string currentDir = Directory.GetCurrentDirectory().Split(new string[] { "\\FrameWork\\" }, StringSplitOptions.None).First();
            return Path.Combine(currentDir, "FrameWork");
        }

        private static bool EncryptFile(string filepath)
        {
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
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Exclamation;
                var result = MessageBox.Show(Application.Current.MainWindow, messageBoxText, caption, button, icon);
                return result.Equals(MessageBoxResult.Yes) ? true : false;
            } 
        }
    }
}
