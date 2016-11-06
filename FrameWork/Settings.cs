using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWork
{
    public static class Settings
    {
        private const string _resourcesFolder = "Resources/ColorSchemes/";

        private static Parameters paramters = null;
        private static string _oldColorScheme = string.Empty;
        private static Dictionary<string, string> _colorSchemeDict = new Dictionary<string, string>() {
            { "Light", _resourcesFolder + "Light.xaml" },
            { "ShinyBlue", _resourcesFolder + "ShinyBlue.xaml" },
            { "Dark", _resourcesFolder + "Dark.xaml" }
        };

        public static event EventHandler OnUIColorSchemeUpdate;

            //AnotherExpressionLight
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

            //ResourceDictionary rd = new ResourceDictionary();
            //rd.Source = new Uri("Resources/ResourceDictionaries/Styles.xaml", UriKind.Relative);
            //Application.Current.Resources.MergedDictionaries.Add(rd);

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

            StaticResources.UpdateCloseTabButtonWidth(rd);
            OnUIColorSchemeUpdate?.Invoke(null, new EventArgs());
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
