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
            CommonInit.Init();
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            CommonInit.CleanUp();
        }

        [TestMethod]
        public void UpdateUIWidthTest()
        {
            Application.ResourceAssembly = Assembly.GetAssembly(typeof(MainWindow));
            var viewModel = new MainWindowViewModel();
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
            while (tabsCount > 0)
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
    }
}
