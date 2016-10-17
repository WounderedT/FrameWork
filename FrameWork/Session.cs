using FrameWork.DataModels;
using FrameWork.ViewModel;
using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FrameWork
{
    public static class Session
    {
        public static event UpdateSelectedTabEventHandler UpdateSelectedTab;
        public delegate void  UpdateSelectedTabEventHandler(EventArgs args);

        private static TabItem _selectedTab;

        public static ObservableCollection<ClosableTab> Tabs { get; set; }
        public static TabItem SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                OnUpdateSelectedTab(new EventArgs());
            }
        }

        public static async void GetSession()
        {
            Tabs = new ObservableCollection<ClosableTab>();
            if (IOProxy.Exists(".session"))
            {
                await LoadSession();
            }
            else
            {
                await NewTab();
            }
            
        }

        public static async Task<bool> SaveSession()
        {
            return await CloseTabAsync();
        }

        private static async Task LoadSession()
        {
            var ms = IOProxy.GetMemoryStreamFromFile(".session");
            BinaryFormatter formatter = new BinaryFormatter();
            List<SessionEntry> lastSessionTabs = (List<SessionEntry>)formatter.Deserialize(ms);
            var plugins = PluginEntryCollection.Plugins;
            Task[] loadTasks = new Task[lastSessionTabs.Count];
            for (int index = 0; index < lastSessionTabs.Count; index++)
            {
                ClosableTab tab = new ClosableTab();
                tab.PluginTabClose += CloseTab;
                Tabs.Add(tab);
            }
            for(int ind = 0; ind < Tabs.Count; ind++)
            {
                if (lastSessionTabs[ind].Name.Equals("Default"))
                    loadTasks[ind] = AddDefaultTab(lastSessionTabs[ind], Tabs[ind]);
                else 
                    loadTasks[ind] = UpdateTabContent(plugins[lastSessionTabs[ind].Name], Tabs[ind], lastSessionTabs[ind]);
            }
            await Task.WhenAll(loadTasks);
            AddNewButtonPlaceholder();
        }

        public static async Task NewTab()
        {
            await AddDefaultTab();
            SelectedTab = Tabs[Tabs.Count - 1];
            AddNewButtonPlaceholder();
        }

        private static async void CloseTab(object sender, PluginTabCloseEventArgs args)
        {
            await CloseTabAsync((ClosableTab)sender);
        }

        public static ClosableTab isOpened(string tabTitle)
        {
            return Tabs.Where(w => w.Title == tabTitle).FirstOrDefault();
        }

        public static ClosableTab GetSelectedTab()
        {
            return Tabs.Where(w => w.IsSelected == true).FirstOrDefault();
        }

        private static async void GetPluginUI(object sender, GetPluginUIRequestEventArgs args)
        {
            ClosableTab requestedPlugin = isOpened(args.SourcePlugin.Name);
            if (requestedPlugin == null)
            {
                int index = Tabs.IndexOf(GetSelectedTab());
                await UpdateTabContent(args.SourcePlugin, Tabs[index]);
            }
            else
            {
                await CloseTabAsync(GetSelectedTab());
                SelectedTab = requestedPlugin;
            }
        }

        private static async Task UpdateTabContent(PluginEntry pluginEntry, ClosableTab tab, SessionEntry? entry = null)
        {
            pluginEntry.Plugin.ProgressReportStart += tab.CreateProgressBar;
            pluginEntry.Plugin.ProgressReportUpdate += tab.UpdateProgressBar;
            pluginEntry.Plugin.ProgressReportComplete += tab.ClearProgressBarEventHandler;
            pluginEntry.Plugin.ReadFromFileRequest += ReadFromFileRequestHandler;
            pluginEntry.Plugin.WriteToFileRequest += WriteToFileRequestHandler;
            await pluginEntry.Plugin.RestoreAsync();
            tab.Content = pluginEntry.Plugin;
            tab.Title = pluginEntry.Name;
            if (entry != null)
            {
                if (((SessionEntry)entry).isSelected)
                {
                    SelectedTab = Tabs[((SessionEntry)entry).Positon];
                }
            }
        }

        private static async Task AddDefaultTab(SessionEntry? entry = null, ClosableTab tab = null)
        {
            await Task.Factory.StartNew(() => {
                DefaultTabViewModel defaultTabVM = new DefaultTabViewModel();
                defaultTabVM.GetPluginUIRequest += GetPluginUI;
                if(tab == null)
                {
                    ClosableTab defaultTab = new ClosableTab() { Title = "Default", Content = defaultTabVM };
                    defaultTab.PluginTabClose += CloseTab;
                    Tabs.Add(defaultTab);
                }
                else
                {
                    tab.Title = "Default";
                    tab.Content = defaultTabVM;
                    if (((SessionEntry)entry).isSelected)
                        SelectedTab = tab;
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static void AddNewButtonPlaceholder()
        {
            NewTabPlaceholder pluginTab = new NewTabPlaceholder();
            pluginTab.NewTabCreationRequest += AlertNewTabRequest;
            Tabs.Add(pluginTab);
        }

        private static async Task<bool> CloseTabAsync(ClosableTab tabToClose = null)
        {
            List<ClosableTab> tabsToClose = new List<ClosableTab>();
            if(tabToClose == null)
                tabsToClose = Tabs.Where(w => !w.Title.Equals("Default") && !w.Title.Equals("Unnamed")).ToList();
            else
                if(!tabToClose.Title.Equals("Default"))
                    tabsToClose.Add(tabToClose);
            List<ClosableTab> busyTabs = new List<ClosableTab>();
            foreach(ClosableTab tab in tabsToClose)
            {
                if (((ITab)tab.Content).isBusy)
                    busyTabs.Add(tab);
            }
            if(busyTabs.Count > 0)
            {
                if(ShowBusyTabsErrorMessage(busyTabs) == MessageBoxResult.No)
                    return false;
                foreach(ClosableTab tab in busyTabs)
                    ((ITab)tab.Content).CancellationToken.Cancel();
            }
            await WriteSessionToFile();
            if(tabToClose != null)
                await DumpAndRemoveTab(tabToClose);
            else
                await DumpAndRemoveTabs(tabsToClose);
            return true;
        }

        private static async Task DumpAndRemoveTab(ClosableTab tab)
        {
            int index = Tabs.IndexOf(tab);
            if (index == Tabs.Count - 2 && tab.IsSelected)
            {
                if (Tabs.Count > 2)
                {
                    SelectedTab = Tabs[index - 1];
                }
            }
            if (!Tabs[index].Title.Equals("Default"))
            {
                await ((ITab)Tabs[index].Content).DumpAsync();
            }
            Tabs.RemoveAt(index);
        }

        private static async Task DumpAndRemoveTabs(List<ClosableTab> tabsToClose)
        {
            Task[] closeTasks = new Task[tabsToClose.Count];
            for (int ind = 0; ind < tabsToClose.Count; ind++)
            {
                closeTasks[ind] = ((ITab)tabsToClose[ind].Content).DumpAsync();
            }
            await Task.WhenAll(closeTasks);
            Tabs.Clear();
        }

        private static async Task WriteSessionToFile()
        {
            List<SessionEntry> openTabsNames = new List<SessionEntry>();
            for(int ind = 0; ind < Tabs.Count - 1; ind++)
            {
                openTabsNames.Add(new SessionEntry(Tabs[ind].Title, ind, Tabs[ind].IsSelected));
            }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, openTabsNames);
            await IOProxy.WriteMemoryStreamToFileAsync(ms, ".session");
        }

        private static MessageBoxResult ShowBusyTabsErrorMessage(List<ClosableTab> busyTabs)
        {
            var tabs = string.Join(Environment.NewLine, busyTabs.Select(w => w.Title).ToArray());
            string messageBoxText = "The following " + GetWarningText(busyTabs.Count) + " still executing tasks:" + Environment.NewLine
                + Environment.NewLine + tabs + Environment.NewLine + Environment.NewLine + "Are you sure you want to forcibly close the program?";
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            return MessageBox.Show(Application.Current.MainWindow, messageBoxText, caption, button, icon);
        }

        private static string GetWarningText(int count)
        {
            return (count == 1) ? ("tab is") : ("tabs are");
        }

        private static void CloseSelectedTab(int? tabIndex = null)
        {
            if(tabIndex == null)
            {
                tabIndex = Tabs.IndexOf(GetSelectedTab());
            }
            Tabs.RemoveAt((int)tabIndex);
        }

        private static async void AlertNewTabRequest(object sender, EventArgs args)
        {
            Tabs.RemoveAt(Tabs.Count - 1);
            await NewTab();
        }

        private static async Task ReadFromFileRequestHandler(object sender, EventArgs args)
        {
            var arguments = (IFileActionRequestEventArgs)args;
            FileActionRequestHandler handler = new FileActionRequestHandler();
            try
            {
                arguments.InOutData = await handler.ReadBytesFromFileAsync(arguments.FileName);
            }
            catch (Exception ex)
            {
                arguments.Exception = ex;
            }
        }

        private static async Task WriteToFileRequestHandler(object sender, EventArgs args)
        {
            var arguments = (IFileActionRequestEventArgs)args;
            FileActionRequestHandler handler = new FileActionRequestHandler();
            try
            {
                await handler.WriteBytesToFileAsync(arguments.FileName, arguments.InOutData);
            }
            catch (Exception ex)
            {
                arguments.Exception = ex;
            }
        }

        private static void OnUpdateSelectedTab(EventArgs args)
        {
            UpdateSelectedTab?.Invoke(args);
        }
    }

    [Serializable]
    public struct SessionEntry
    {
        public string Name { get; set; }
        public int Positon { get; set; }
        public bool isSelected { get; set; }

        public SessionEntry(string name, int position, bool isSelected)
        {
            Name = name;
            Positon = position;
            this.isSelected = isSelected;
        }
    }
}
