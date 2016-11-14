using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FrameWork;
using FrameWork.ViewModel;
using System.Windows;
using System.Reflection;
using System.IO;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class FrameWorkUnitTests
    {
        [TestMethod]
        public void UpdateUIWidthTest()
        {
            if (Application.Current == null)
            { new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }

            string path = Directory.GetCurrentDirectory();
            var pathArray = path.Split(new string[] { "FrameWorkUnitTests" }, StringSplitOptions.None);
            Directory.SetCurrentDirectory(pathArray[0] + "FrameWork" + pathArray[1]);

            Application.ResourceAssembly = Assembly.GetAssembly(typeof(MainWindow));


            var app = new MainWindow();
            app.InitializeComponent();

            var some = app.DataContext as MainWindowViewModel;
            var width = StaticResources.DynamicWindowAreaWidth;
        }
    }
}
