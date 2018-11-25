using FrameWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class CommonInit
    {
        private static string _currentDir = Directory.GetCurrentDirectory();

        internal static void Init()
        {
            if (Application.Current == null)
            { new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }

            if (_currentDir.Contains("\\FrameWorkUnitTests\\"))
            {
                Directory.SetCurrentDirectory(_currentDir.Replace("FrameWorkUnitTests", "FrameWork"));
            }
        }

        internal static void LoadSettings()
        {
            Application.ResourceAssembly = System.Reflection.Assembly.GetAssembly(typeof(MainWindow));
            Settings.LoadSettings();
        }

        internal static void CleanUp()
        {
            Directory.SetCurrentDirectory(_currentDir);
            Application.Current.Shutdown();
        }
    }
}
