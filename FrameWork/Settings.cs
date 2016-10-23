using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FrameWork
{
    public static class Settings
    {
        private static Parameters paramters = null;

        public static bool EncryptFiles
        {
            get { return paramters.EncryptFiles; }
            set { paramters.EncryptFiles = value; }
        }

        public static object CurrentColorScheme
        {
            get { return paramters.CurrentColorScheme; }
            set { paramters.CurrentColorScheme = value; }
        }

        public static List<object> ColorSchemes
        {
            get { return paramters.ColorSchemes; }
            set { paramters.ColorSchemes = value; }
        }

        public static void LoadSettings()
        {
            paramters = new Parameters();
            if (IOProxy.Exists(".config"))
                paramters.Deserialize(IOProxy.GetMemoryStreamFromFile(".config"));
        }

        public static async void SaveSettings()
        {
            await IOProxy.WriteMemoryStreamToFileAsync(paramters.Serialize(), ".config");
        }
    }

    [Serializable]
    class Parameters
    {
        public bool EncryptFiles { get; set; }
        public object CurrentColorScheme { get; set; }
        public List<object> ColorSchemes { get; set; }

        public Parameters()
        {
            EncryptFiles = true;
            CurrentColorScheme = new object();
            ColorSchemes = new List<object>();
        }

        public MemoryStream Serialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, this);
            return ms;
        }

        public void Deserialize(MemoryStream ms)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            var obj = (Parameters)formatter.Deserialize(ms);
            EncryptFiles = obj.EncryptFiles;
            CurrentColorScheme = obj.CurrentColorScheme;
            ColorSchemes = obj.ColorSchemes;
        }
    }
}
