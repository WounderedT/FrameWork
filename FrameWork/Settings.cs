using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace FrameWork
{
    public static class Settings
    {
        private const string _resourcesFolder = "Resources/ColorSchemes/";
        private static Dictionary<string, string> _colorSchemeDict = new Dictionary<string, string>() {
            { "Light", _resourcesFolder + "Light.xaml" },
            { "Dark", _resourcesFolder + "Dark.xaml" }
        };

        private static Parameters paramters = null;
        private static string _oldColorScheme = string.Empty;

        public static string CryptographyLibFileName
        {
            get { return "Crypto.dll";}
        }

        public static string InterfacesLibFileName
        {
            get { return "interface.dll"; }
        }

        public static bool EncryptFiles
        {
            get { return paramters.EncryptFiles; }
            set { paramters.EncryptFiles = value; }
        }

        public static string CurrentColorScheme
        {
            get { return paramters.CurrentColorScheme; }
            set {
                if(paramters.CurrentColorScheme != value)
                {
                    _oldColorScheme = paramters.CurrentColorScheme;
                    paramters.CurrentColorScheme = value;
                    UpdateUIColorScheme();
                }
            }
        }

        public static List<string> ColorSchemes
        {
            get { return _colorSchemeDict.Keys.ToList(); }
        }

        public static bool IsColorSchemeUpdated { get; set; }

        public static void LoadSettings()
        {
            paramters = new Parameters();
            if (IOProxy.Exists(".config"))
                paramters.Deserialize(IOProxy.GetMemoryStreamFromFile(".config"));

            if (string.IsNullOrEmpty(CurrentColorScheme))
                CurrentColorScheme = _colorSchemeDict.First().Key;
            else
                UpdateUIColorScheme();
        }

        public static void UpdateUIColorScheme()
        {
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri(_colorSchemeDict[CurrentColorScheme], UriKind.Relative);
            if (!string.IsNullOrEmpty(_oldColorScheme))
                Application.Current.Resources.MergedDictionaries.Remove(
                    Application.Current.Resources.MergedDictionaries.Where(w => w.Source.OriginalString.Equals(_colorSchemeDict[_oldColorScheme])).First()
                    );
            Application.Current.Resources.MergedDictionaries.Add(rd);
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
        public string CurrentColorScheme { get; set; }

        public Parameters()
        {
            EncryptFiles = true;
            CurrentColorScheme = string.Empty;
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
        }
    }
}
