using FrameWork.DataModels;
using FrameWork.ViewModel;
using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public static event EventHandler UpdateDragAreaWidth;
        public delegate void UpdateSelectedTabEventHandler(EventArgs args);

        private static int _selectedTabIndex;
        private static Dictionary<string, object> _resources = new Dictionary<string, object>();

        public static ObservableCollection<ClosableTab> Tabs { get; set; }
        public static int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                OnUpdateSelectedTab(new EventArgs());
            }
        }

        public static async Task<ObservableCollection<ClosableTab>> GetSession()
        {
            Tabs = new ObservableCollection<ClosableTab>();
            Tabs.CollectionChanged += UpdateUIWidth;
            if (IOProxy.Exists(".session"))
                await LoadSession();
            else
                await NewTab();
            return Tabs;
        }

        public static async Task<bool> SaveSession()
        {
            return await CloseTabAsync();
        }

        private static async Task LoadSession()
        {
            var encryptedStream = IOProxy.GetMemoryStreamFromFile(".session");
            var array = await Authentification.Cryptography.DecryptMemoryStreamAsync(encryptedStream,
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt);
            MemoryStream ms = new MemoryStream();
            await ms.WriteAsync(array, 0, array.Length);
            ms.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            List<SessionEntry> lastSessionTabs = (List<SessionEntry>)formatter.Deserialize(ms);
            if (lastSessionTabs.Count == 0)
            {
                await AddDefaultTab();
                return;
            }
            var plugins = PluginEntryCollection.Plugins;
            Task[] loadTasks = new Task[lastSessionTabs.Count];
            for (int index = 0; index < lastSessionTabs.Count; index++)
            {
                ClosableTab tab = new ClosableTab();
                tab.PluginTabClose += CloseTab;
                Tabs.Add(tab);
            }
            for (int ind = 0; ind < Tabs.Count; ind++)
            {
                if (isSystemTab(lastSessionTabs[ind].Name))
                    loadTasks[ind] = LoadSystemTab(lastSessionTabs[ind], Tabs[ind]);
                else
                    loadTasks[ind] = UpdateTabContent(plugins[lastSessionTabs[ind].Name], Tabs[ind], lastSessionTabs[ind]);
            }
            await Task.WhenAll(loadTasks);
        }

        public static async Task NewTab()
        {
            await AddDefaultTab();
            SelectedTabIndex = Tabs.Count - 1;
            //UpdateUIWidth();
        }

        public static void UpdateUIWidth(object sender, NotifyCollectionChangedEventArgs e)
        {
            double width = 0;
            foreach (ClosableTab entry in Tabs)
            {
                if(entry.ActualWidth == 0)
                    width += StaticResources.TabTitleDefaultWidth + StaticResources.NewTabButtonSize;
                else
                    width += entry.ActualWidth;
            }
            if (width > StaticResources.TabAreaWidth)
            {
                UpdateDragAreaWidth?.Invoke(null, new UpdateDragAreaWidthEventArgs(StaticResources.WindowDragAreaMinWidth));
                width = StaticResources.TabAreaWidth / Tabs.Count - StaticResources.TabCloseButtonWidth - StaticResources.TabHeaderPadding;
                foreach (ClosableTab entry in Tabs)
                {
                    int some = Tabs.IndexOf(entry);
                    entry.HeaderWidth = width;
                }
                return;
            }
            if (width < StaticResources.TabAreaWidth)
            {
                width = StaticResources.TabAreaWidth / Tabs.Count;
                if (width > StaticResources.TabHeaderDefaultWidth)
                    width = StaticResources.TabHeaderDefaultWidth;
                UpdateDragAreaWidth?.Invoke(null, new UpdateDragAreaWidthEventArgs(
                    StaticResources.MainWindowWidth - width * Tabs.Count - StaticResources.SystemButtonAreaWidth));
                width = width - StaticResources.TabCloseButtonWidth - StaticResources.TabHeaderPadding;
                foreach (ClosableTab entry in Tabs)
                {
                    int some = Tabs.IndexOf(entry);
                    entry.HeaderWidth = width;
                }
                return;
            }
        }

        private static async void CloseTab(object sender, PluginTabCloseEventArgs args)
        {
            await CloseTabAsync((ClosableTab)sender);
        }

        public static async void CloseTab()
        {
            await CloseTabAsync(GetSelectedTab());
        }

        public static ClosableTab isOpened(string tabTitle)
        {
            return Tabs.Where(w => w.Title == tabTitle).FirstOrDefault();
        }

        public static ClosableTab GetSelectedTab()
        {
            return Tabs.Where(w => w.IsSelected == true).FirstOrDefault();
        }

        public static bool isSystemTab(string name)
        {
            if(name.Equals("Default") || name.Equals("Settings"))
                return true;
            return false;
        }

        public static Task LoadSystemTab(SessionEntry entry, ClosableTab tab)
        {
            Task load = null;
            switch (entry.Name)
            {
                case "Default":
                    load = AddDefaultTab(entry, tab);
                    break;
                case "Settings":
                    load = AddSettingsTab(tab, entry);
                    break;
            }
            return load;
        }

        public static async void GetPluginUI(PluginEntry sourcePlugin)
        {
            ClosableTab requestedPlugin = isOpened(sourcePlugin.Name);
            if (requestedPlugin == null)
            {
                await UpdateTabContent(sourcePlugin, GetSelectedTab());
            }
            else
            {
                await CloseTabAsync(GetSelectedTab());
                SelectedTabIndex = Tabs.IndexOf(requestedPlugin);
            }
        }

        public static async void GetSettingsTab()
        {
            ClosableTab settingsTab = isOpened("Settings");
            if (settingsTab == null)
            {
                await AddSettingsTab(GetSelectedTab());
            }
            else
            {
                await CloseTabAsync(GetSelectedTab());
                SelectedTabIndex = Tabs.IndexOf(settingsTab);
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
                    SelectedTabIndex = ((SessionEntry)entry).Positon;
                }
            }
        }

        private static async Task AddSettingsTab(ClosableTab tab, SessionEntry? entry = null)
        {
            await Task.Factory.StartNew(() => {
                tab.Title = "Settings";
                tab.Content = new SettingsViewModel();
                if(entry != null)
                    if (((SessionEntry)entry).isSelected)
                        SelectedTabIndex = Tabs.IndexOf(tab);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static async Task AddDefaultTab(SessionEntry? entry = null, ClosableTab tab = null)
        {
            await Task.Factory.StartNew(() => {
                DefaultTabViewModel defaultTabVM = new DefaultTabViewModel();
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
                        SelectedTabIndex = Tabs.IndexOf(tab);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static void AddNewButtonPlaceholder()
        {
            //NewTabPlaceholder pluginTab = new NewTabPlaceholder();
            //pluginTab.NewTabCreationRequest += AlertNewTabRequest;
            //Tabs.Add(pluginTab);
        }

        private static async Task<bool> CloseTabAsync(ClosableTab tabToClose = null)
        {
            List<ClosableTab> tabsToClose = new List<ClosableTab>();
            if(tabToClose == null)
                tabsToClose = Tabs.Where(w => !w.Title.Equals("Unnamed") && !isSystemTab(w.Title)).ToList();
            else
                if(!isSystemTab(tabToClose.Title))
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
                    SelectedTabIndex = index - 1;
                }
            }
            if (!isSystemTab(Tabs[index].Title))
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
            byte[] array = await Authentification.Cryptography.EncryptDataArrayAsync(ms.ToArray(),
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt);
            MemoryStream encryptedMS = new MemoryStream();
            await encryptedMS.WriteAsync(array, 0, array.Length);
            await IOProxy.WriteMemoryStreamToFileAsync(encryptedMS, ".session");
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

        public static async void NewTabRequest()
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

    public class UpdateDragAreaWidthEventArgs: EventArgs
    {
        public double Width { get; set; }

        public UpdateDragAreaWidthEventArgs(double width)
        {
            Width = width;
        }
    }
}
