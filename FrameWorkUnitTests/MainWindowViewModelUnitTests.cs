using FrameWork;
using FrameWork.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class MainWindowViewModelUnitTests
    {
        private static string _currentDir = Directory.GetCurrentDirectory();

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            if (Application.Current == null)
            { new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }

            if (_currentDir.Contains("\\FrameWorkUnitTests\\"))
            {
                var pathArray = _currentDir.Split(new string[] { "FrameWorkUnitTests" }, StringSplitOptions.None);
                Directory.SetCurrentDirectory(pathArray[0] + "FrameWork" + pathArray[1]);
            }
        }

        [TestMethod]
        public void UpdateUIWidthTest()
        {
            Application.ResourceAssembly = Assembly.GetAssembly(typeof(MainWindow));
            var app = new MainWindow();
            app.InitializeComponent();
            var viewModel = app.DataContext as MainWindowViewModel;

            viewModel.Tabs.Clear();
            viewModel.NewTabButtonVisibility = Visibility.Visible;

            viewModel.Tabs.Add(new FrameWork.DataModels.ClosableTab());
            viewModel.UpdateUIWidth(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            double dragArea = StaticResources.DynamicWindowAreaWidth - StaticResources.TabTitleDefaultWidth;
            Assert.AreEqual(dragArea, viewModel.WindowDragAreaWidth);

            viewModel.Tabs.Clear();
            viewModel.UpdateUIWidth(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            Assert.AreEqual(StaticResources.MainWindowWidth - StaticResources.SystemButtonAreaWidth, viewModel.WindowDragAreaWidth);

            int tabsCount = (int)(StaticResources.TabAreaWidth / StaticResources.TabTitleDefaultWidth) * 2;
            while(tabsCount > 0)
            {
                viewModel.Tabs.Add(new FrameWork.DataModels.ClosableTab());
                tabsCount--;
            }
            viewModel.UpdateUIWidth(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            Assert.AreEqual(StaticResources.WindowDragAreaMinWidth, viewModel.WindowDragAreaWidth);
            double tabWidth = StaticResources.TabAreaWidth / viewModel.Tabs.Count;
            foreach (FrameWork.DataModels.ClosableTab tab in viewModel.Tabs)
            {
                Assert.AreEqual(tabWidth, tab.HeaderWidth);
            }
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            Directory.SetCurrentDirectory(_currentDir);
            Application.Current.Shutdown();
        }
    }
}
