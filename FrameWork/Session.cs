using FrameWork.DataModels;
using FrameWork.ViewModel;
using Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWork
{
    public static class Session
    {
        public static event EventHandler UpdateSelectedTab;

        public static async Task GetSession()
        {
            if (IOProxy.Exists(".session"))
                await LoadSession();
            else
                await NewTab();
        }

        public static async Task<bool> SaveSession()
        {
            return await CloseTabAsync();
        }

        private static async Task LoadSession()
        {
            var encryptedStream = IOProxy.GetMemoryStreamFromFile(".session");
            byte[] array = new byte[1];
            try
            {
                array = await Authentification.Cryptography.DecryptMemoryStreamAsync(encryptedStream,
                        Authentification.AppPassword.Password, Authentification.AppPassword.Salt);
            }
            catch (System.Security.SecurityException)
            {
                await NewTab();
                return;
            }
            MemoryStream ms = new MemoryStream();
            await ms.WriteAsync(array, 0, array.Length);
            ms.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            List<SessionEntry> lastSessionTabs = (List<SessionEntry>)formatter.Deserialize(ms);
            if (lastSessionTabs.Count == 0)
            {
                await NewTab();
                return;
            }
            var plugins = PluginEntryCollection.Plugins;
            foreach (var entry in lastSessionTabs.ToArray())
            {
                if ((!IsSystemTab(entry.Name)) && !plugins.ContainsKey(entry.Name))
                {
                    lastSessionTabs.Remove(lastSessionTabs.Where(w => w.Equals(entry)).FirstOrDefault());
                }
            }
            Task[] loadTasks = new Task[lastSessionTabs.Count];
            for (int index = 0; index < lastSessionTabs.Count; index++)
            {
                ClosableTab tab = new ClosableTab();
                tab.PluginTabClose += CloseTab;
                MainWindowViewModel.tabs.Add(tab);
            }
            for (int ind = 0; ind < MainWindowViewModel.tabs.Count; ind++)
            {
                if (IsSystemTab(lastSessionTabs[ind].Name))
                    loadTasks[ind] = LoadSystemTab(lastSessionTabs[ind], MainWindowViewModel.tabs[ind]);
                else
                    loadTasks[ind] = UpdateTabContent(plugins[lastSessionTabs[ind].Name], MainWindowViewModel.tabs[ind], lastSessionTabs[ind]);
            }
            await Task.WhenAll(loadTasks);
        }

        public static async Task NewTab()
        {
            await AddDefaultTab();
            OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(MainWindowViewModel.tabs.Count - 1));
        }

        private static async void CloseTab(object sender, PluginTabCloseEventArgs args)
        {
            int? newSelectedTab = null;
            if ((sender as ClosableTab).IsSelected)
                newSelectedTab = GetNextSelectedTabIndex(GetSelectedTab());
            await CloseTabAsync((ClosableTab)sender);
            if (newSelectedTab != null)
                OnUpdateSelectedTab(new UpdateSelectedTabEventArgs((int)newSelectedTab));
        }

        public static async void CloseTab()
        {
            int newSelectedTab = GetNextSelectedTabIndex(GetSelectedTab());
            await CloseTabAsync(GetSelectedTab());
            OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(newSelectedTab));
        }

        public static ClosableTab isOpened(string tabTitle)
        {
            return MainWindowViewModel.tabs.Where(w => w.Title == tabTitle).FirstOrDefault();
        }

        public static ClosableTab GetSelectedTab()
        {
            return MainWindowViewModel.tabs.Where(w => w.IsSelected == true).FirstOrDefault();
        }

        public static bool IsSystemTab(string name)
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
                await UpdateTabContent(sourcePlugin, GetSelectedTab());
            else
            {
                await CloseTabAsync(GetSelectedTab());
                OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(MainWindowViewModel.tabs.IndexOf(requestedPlugin)));
            }
        }

        public static async void GetSettingsTab()
        {
            ClosableTab settingsTab = isOpened("Settings");
            if (settingsTab == null)
                await AddSettingsTab(GetSelectedTab());
            else
            {
                await CloseTabAsync(GetSelectedTab());
                OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(MainWindowViewModel.tabs.IndexOf(settingsTab)));
            }
        }        

        private static async Task UpdateTabContent(PluginEntry pluginEntry, ClosableTab tab, SessionEntry? entry = null)
        {
            pluginEntry.Plugin.ProgressReportStart += tab.CreateProgressBar;
            pluginEntry.Plugin.ProgressReportUpdate += tab.UpdateProgressBar;
            pluginEntry.Plugin.ProgressReportComplete += tab.ClearProgressBarEventHandler;
            pluginEntry.Plugin.ReadFromFileRequest += ReadFromFileRequestHandler;
            pluginEntry.Plugin.WriteToFileRequest += WriteToFileRequestHandler;
            try
            {
                await pluginEntry.Plugin.RestoreAsync();
            }
            catch (System.Security.SecurityException)
            {
                string message = "Password has been changed. " + pluginEntry.Name + " plugin state cannot be loaded.";
                string caption = "Plugin state load error";
                var result = MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            tab.Content = pluginEntry.Plugin;
            tab.Title = pluginEntry.Name;
            if (entry != null)
            {
                if (((SessionEntry)entry).isSelected)
                {
                    OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(((SessionEntry)entry).Positon));
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
                        OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(MainWindowViewModel.tabs.IndexOf(tab)));
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
                    MainWindowViewModel.tabs.Add(defaultTab);
                }
                else
                {
                    tab.Title = "Default";
                    tab.Content = defaultTabVM;
                    if (((SessionEntry)entry).isSelected)
                        OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(MainWindowViewModel.tabs.IndexOf(tab)));
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static async Task<bool> CloseTabAsync(ClosableTab tabToClose = null)
        {
            List<ClosableTab> tabsToClose = new List<ClosableTab>();
            if(tabToClose == null)
                tabsToClose = MainWindowViewModel.tabs.Where(w => !IsSystemTab(w.Title)).ToList();
            else
                if(!IsSystemTab(tabToClose.Title))
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
            if(tabToClose != null)
                await DumpAndRemoveTab(tabToClose);
            else
            {
                await WriteSessionToFile();
                await DumpAndRemoveTabs(tabsToClose);
            }
            return true;
        }

        private static async Task DumpAndRemoveTab(ClosableTab tab)
        {
            int index = MainWindowViewModel.tabs.IndexOf(tab);
            if (index == MainWindowViewModel.tabs.Count - 2 && tab.IsSelected)
            {
                if (MainWindowViewModel.tabs.Count > 2)
                {
                    OnUpdateSelectedTab(new UpdateSelectedTabEventArgs(index - 1));
                }
            }
            if (!IsSystemTab(MainWindowViewModel.tabs[index].Title))
            {
                await ((ITab)MainWindowViewModel.tabs[index].Content).DumpAsync();
            }
            MainWindowViewModel.tabs.RemoveAt(index);
        }

        private static async Task DumpAndRemoveTabs(List<ClosableTab> tabsToClose)
        {
            Task[] closeTasks = new Task[tabsToClose.Count];
            for (int ind = 0; ind < tabsToClose.Count; ind++)
            {
                closeTasks[ind] = ((ITab)tabsToClose[ind].Content).DumpAsync();
            }
            await Task.WhenAll(closeTasks);
            MainWindowViewModel.tabs.Clear();
        }

        private static async Task WriteSessionToFile()
        {
            List<SessionEntry> openTabsNames = new List<SessionEntry>();
            for(int ind = 0; ind < MainWindowViewModel.tabs.Count; ind++)
            {
                openTabsNames.Add(new SessionEntry(MainWindowViewModel.tabs[ind].Title, ind, MainWindowViewModel.tabs[ind].IsSelected));
            }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, openTabsNames);
            byte[] array = await Authentification.Cryptography.EncryptDataArrayAsync(ms.ToArray(),
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt).ConfigureAwait(false);
            MemoryStream encryptedMS = new MemoryStream();
            await encryptedMS.WriteAsync(array, 0, array.Length).ConfigureAwait(false);
            await IOProxy.WriteMemoryStreamToFileAsync(encryptedMS, ".session").ConfigureAwait(false);
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
                tabIndex = MainWindowViewModel.tabs.IndexOf(GetSelectedTab());
            }
            MainWindowViewModel.tabs.RemoveAt((int)tabIndex);
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

        private static int GetNextSelectedTabIndex(ClosableTab tab)
        {
            int newSelectedTab = 0;
            if (MainWindowViewModel.tabs.Count > 1)
                if (MainWindowViewModel.tabs.IndexOf(tab) == MainWindowViewModel.tabs.Count - 1)
                    newSelectedTab = MainWindowViewModel.tabs.Count - 2;
                else
                    newSelectedTab = MainWindowViewModel.tabs.IndexOf(tab);
            return newSelectedTab;
        }

        private static void OnUpdateSelectedTab(UpdateSelectedTabEventArgs args)
        {
            UpdateSelectedTab?.Invoke(null, args);
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

    public class UpdateSelectedTabEventArgs: EventArgs
    {
        public int SelectedTab { get; set; }

        public UpdateSelectedTabEventArgs(int selectedTab)
        {
            SelectedTab = selectedTab;
        }
    }
}
