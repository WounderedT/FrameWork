using FrameWork.DataModels;
using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FrameWork.ViewModel
{
    /*
    TODO:
    2. Remove default tab init from this. Put into Session.
    */
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        private static string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";

        private Visibility _newTabButtonVisibility;

        private int _selectedTabIndex = 0;
        private double _windowDragAreaWidth;

        private RelayCommandAsync _newTabCommand;
        private RelayCommand _minimizeMainWindowCommand;
        private RelayCommand _closeMainWindowCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        public RelayCommand<DragEventArgs> CloseCommand { get; private set; }
        public System.Threading.Mutex mutex = new System.Threading.Mutex(false, appGuid);
        public static ObservableCollection<ClosableTab> tabs = new ObservableCollection<ClosableTab>();

        public ObservableCollection<ClosableTab> Tabs
        {
            get { return tabs; }
            set
            {
                tabs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tabs"));
            }
        }

        public TabItem SelectedTab
        {
            get
            {
                if (Tabs == null || Tabs.Count == 0 || SelectedTabIndex < 0)
                    return null;
                return Tabs[SelectedTabIndex];
            }
            set
            {
                SelectedTabIndex = Tabs.IndexOf(value as ClosableTab);
            }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedTab"));
            }
        }

        public Visibility NewTabButtonVisibility
        {
            get { return _newTabButtonVisibility; }
            set
            {
                if(_newTabButtonVisibility != value)
                {
                    _newTabButtonVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewTabButtonVisibility"));
                }
            }
        }

        public double WindowDragAreaWidth
        {
            get { return _windowDragAreaWidth; }
            set
            {
                if(_windowDragAreaWidth != value)
                {
                    _windowDragAreaWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WindowDragAreaWidth"));
                }
            }
        }

        public ICommand NewTabCommand
        {
            get
            {
                if (_newTabCommand == null)
                {
                    _newTabCommand = new RelayCommandAsync(param => NewTabRequest());
                }
                return _newTabCommand;
            }
        }

        public ICommand MinimizeMainWindowCommand
        {
            get
            {
                if (_minimizeMainWindowCommand == null)
                {
                    _minimizeMainWindowCommand = new RelayCommand(param => MinimizeMainWindow());
                }
                return _minimizeMainWindowCommand;
            }
        }

        public ICommand CloseMainWindowCommand
        {
            get
            {
                if (_closeMainWindowCommand == null)
                {
                    _closeMainWindowCommand = new RelayCommand(param => CloseMainWindow());
                }
                return _closeMainWindowCommand;
            }
        }

        public async Task NewTabRequest()
        {
            await Session.NewTab();
        }

        public void MinimizeMainWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        public MainWindowViewModel()
        {
            string result = CheckResources();
            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show(GetMissingDllErrorMessage(result), "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CloseMainWindow();
                return;
            }
            StaticResources.InitializeResources();
            Settings.LoadSettings();

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            NewTabButtonVisibility = Visibility.Collapsed;
        }

        public void LoadAuthentificationTab()
        {
            Authentification.GetAuthentificationTab();
            SelectedTabIndex = 0;
            Authentification.AuthentificationComplete += LoadFrameWorkTabs;
            WindowDragAreaWidth = StaticResources.MainWindowWidth - StaticResources.TabTitleDefaultWidth - StaticResources.SystemButtonWidth * 2;
        }

        public async void LoadFrameWorkTabs(AuthentificationEventArgs args)
        {
            if (!args.Result)
            {
                CloseMainWindow();
                return;
            }
            tabs.RemoveAt(0);
            tabs.CollectionChanged += UpdateUIWidth;
            Session.UpdateSelectedTab += UpdateSelectedTab;

            await Session.GetSession();
            NewTabButtonVisibility = Visibility.Visible;
        }

        public async void CloseMainWindow()
        {
            if (Tabs.Count == 0 || Tabs[0].Title.Equals("Authentification"))
            {
                Application.Current.Shutdown();
                return;
            }
            if (await Session.SaveSession())
                Application.Current.Shutdown();
        }

        public void UpdateUIWidth(object sender, NotifyCollectionChangedEventArgs e)
        {
            double width = 0;
            foreach (ClosableTab entry in tabs)
            {
                if (entry.ActualWidth == 0)
                    width += StaticResources.TabTitleDefaultWidth;
                else
                    width += entry.ActualWidth;
            }
            if (width > StaticResources.TabAreaWidth)
            {
                width = StaticResources.TabAreaWidth / tabs.Count;
                foreach (ClosableTab entry in tabs)
                    entry.HeaderWidth = width;
                WindowDragAreaWidth = StaticResources.WindowDragAreaMinWidth;
                return;
            }
            if (width < StaticResources.TabAreaWidth)
            {
                width = StaticResources.TabAreaWidth / tabs.Count;
                if (width > StaticResources.TabTitleDefaultWidth)
                    width = StaticResources.TabTitleDefaultWidth;
                WindowDragAreaWidth = StaticResources.DynamicWindowAreaWidth - width * tabs.Count;
                if (!e.Action.Equals(NotifyCollectionChangedAction.Add))
                    foreach (ClosableTab entry in tabs)
                        entry.HeaderWidth = width;
                return;
            }
        }

        private void UpdateSelectedTab(object sender, EventArgs args)
        {
            SelectedTabIndex = ((UpdateSelectedTabEventArgs)args).SelectedTab;
        }

        private string CheckResources()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string delemiter = " and ";
            StringBuilder builder = new StringBuilder();
            if (!File.Exists(Path.Combine(currentDir, Settings.CryptographyLibFileName)))
                builder.Append(Settings.CryptographyLibFileName);
            if (!File.Exists(Path.Combine(currentDir, Settings.InterfacesLibFileName)))
            {
                if(builder.Length > 0)
                    builder.Append(delemiter);
                builder.Append(Settings.InterfacesLibFileName);
            }
            return builder.ToString();
        }

        private string GetMissingDllErrorMessage(string result)
        {
            var dlls = result.Split(new string[] { " and " }, StringSplitOptions.None);
            string[] words = new string[] { "file" };
            if(dlls.Length > 1)
                words = new string[] { "files" };
            return "DLL " + words[0] + " " + result + " cannot be found!";
        }
    }
}
