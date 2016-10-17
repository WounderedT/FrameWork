﻿using FrameWork.ViewModel;
using Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FrameWork.DataModels
{
    public static class PluginEntryCollection
    {
        private static Dictionary<string, PluginEntry> _plugins;
        public static Dictionary<string, PluginEntry> Plugins
        {
            get
            {
                if(_plugins == null)
                {
                    _plugins = new Dictionary<string, PluginEntry>();
                    GetAvailablePlugins();
                }
                return _plugins;
            }
        }

        private static void GetAvailablePlugins()
        {
            var pluginsLocations = Directory.GetDirectories(IOProxy.GetCurrentProjectFolder()).Where(w => w.Contains("Plugin"));
            foreach(string pluginPath in pluginsLocations)
            {
                foreach (string pluginDll in Directory.GetFiles(Path.Combine(pluginPath, @"bin\Debug"), "*.dll"))
                {
                    if (pluginDll.Contains("Interface.dll"))
                    {
                        continue;
                    }
                    var DLL = Assembly.LoadFile(pluginDll);
                    BitmapSource image = GetPLuginPreviewImage(DLL);
                    foreach (Type type in DLL.GetExportedTypes())
                    {
                        try
                        {
                            var entry = new PluginEntry((ITab)Activator.CreateInstance(type), image);
                            _plugins.Add(entry.Name, entry);
                            ResourceDictionary rd = new ResourceDictionary();
                            rd.Source = new Uri("pack://application:,,,/"
                                + DLL.ManifestModule.Name.Split(new string[] { ".dll" }, StringSplitOptions.None).First()
                                + ";component/PluginResourceDictionary.xaml");
                            var some = Application.Current.Resources.MergedDictionaries;
                            some.Add(rd);
                        }
                        catch (InvalidCastException) { }
                        catch (MissingMethodException) { }
                    }
                }
            }
        }

        private static BitmapSource GetPLuginPreviewImage(Assembly assembly)
        {
            var some = assembly.GetManifestResourceNames();
            string previewImagePath = some.Where(w => w.Contains("PluginPreviewImage")).FirstOrDefault();
            Stream imageStream;
            if (previewImagePath != null)
            {
                imageStream = assembly.GetManifestResourceStream(previewImagePath);
                return BitmapToBitmapSource((Bitmap)Image.FromStream(imageStream));
            }
            else
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultPluginPreview.png"));
            }
        }

        private static BitmapSource BitmapToBitmapSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
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
