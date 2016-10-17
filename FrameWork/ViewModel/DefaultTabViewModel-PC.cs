using FrameWork.UC;
using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FrameWork.ViewModel
{
    public class DefaultTabViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<PluginEntry> Plugins { get; set; }

        private string pluginsLocation = @"F:\SkyDrive\Study\C# Projects\FrameWork\TestPlugin\bin\Debug";

        public ObservableCollection<PluginButtonUC> PluginButtons { get; set; } 

        public DefaultTabViewModel()
        {
            Plugins = new List<PluginEntry>();
            PluginButtons = new ObservableCollection<PluginButtonUC>();
            GetAvailablePlugins();
            GetPluginButtons();
        }

        private void GetAvailablePlugins()
        {
            foreach(string pluginDll in Directory.GetFiles(pluginsLocation, "*.dll"))
            {
                if (pluginDll.Contains("Interface.dll"))
                {
                    continue;
                }
                var DLL = Assembly.LoadFile(pluginDll);
                foreach (Type type in DLL.GetExportedTypes())
                {
                    try
                    {                       
                        Plugins.Add(new PluginEntry((ITab)Activator.CreateInstance(type), GetPLuginPreviewImage(DLL)));
                    }
                    catch (InvalidCastException) { }
                }
            }
        }

        private void GetPluginButtons()
        {
            foreach(PluginEntry entry in Plugins)
            {
                PluginButtonUC button = new PluginButtonUC();
                button.viewModel.PluginButtonSourceID = entry.Plugin.Name;
                button.viewModel.PluginButtonImage = entry.PreviewImage;
                PluginButtons.Add(button);
            }
        }

        private BitmapSource GetPLuginPreviewImage(Assembly assembly)
        {
            var some = assembly.GetManifestResourceNames();
            string previewImagePath = some.Where(w => w.Contains("PluginPreviewImage1")).FirstOrDefault();
            Stream imageStream;
            if(previewImagePath != null)
            {
                imageStream = assembly.GetManifestResourceStream(previewImagePath);
                return BitmapToBitmapSource((Bitmap)Image.FromStream(imageStream));
            }
            else
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultPluginPreview.png"));
            }
            
        }

        public BitmapSource BitmapToBitmapSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, 
                BitmapSizeOptions.FromEmptyOptions());
        }

        public event GetPluginUIRequestEventHalder GetPluginUIRequest;
        public delegate void GetPluginUIRequestEventHalder(object sender, GetPluginUIRequestEventArgs args);

        public void EnablePluginUI(string sourcePluginName)
        {
            foreach(PluginEntry entry in Plugins)
            {
                if(entry.Name == sourcePluginName)
                {
                    OnGetPluginUIReques(new GetPluginUIRequestEventArgs(entry.Plugin));
                }
            }
            //for(int ind = 0; ind < Plugins.Count; ind++)
            //{

            //}
        }

        protected virtual void OnGetPluginUIReques(GetPluginUIRequestEventArgs args)
        {
            GetPluginUIRequest?.Invoke(this, args);
        }
    }

    public class GetPluginUIRequestEventArgs: EventArgs
    {
        public ITab SourcePlugin
        {
            get; set;
        }

        public GetPluginUIRequestEventArgs(ITab sourcePlugin)
        {
            SourcePlugin = sourcePlugin;
        }
    }

    public struct PluginEntry
    {
        public ITab Plugin { get; set; }
        public BitmapSource PreviewImage { get; set; }
        public string Name { get; set; }

        public PluginEntry(ITab plugin, BitmapSource previewImage)
        {
            Plugin = plugin;
            PreviewImage = previewImage;
            Name = plugin.Name;
        }
    }
}
