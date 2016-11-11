using FrameWork.DataModels;
using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    class MainWindowViewModel: INotifyPropertyChanged
    {
        public System.Threading.Mutex mutex = new System.Threading.Mutex(false, appGuid);
        private static string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";

        public event PropertyChangedEventHandler PropertyChanged;
        public RelayCommand<DragEventArgs> CloseCommand { get; private set; }

        private ObservableCollection<ClosableTab> _tabs;
        private int _selectedTabIndex = 0;

        private Visibility _newTabButtonVisibility;
        private double _windowDragAreaWidth;

        private RelayCommandAsync _newTabCommand;
        private RelayCommand _minimizeMainWindowCommand;
        private RelayCommand _closeMainWindowCommand;

        public ObservableCollection<ClosableTab> Tabs
        {
            get { return _tabs; }
            set
            {
                _tabs = value;
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
            Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive).WindowState = WindowState.Minimized;
        }

        public MainWindowViewModel()
        {
            StaticResources.InitializeResources();
            //ResourceDictionary rd = new ResourceDictionary();
            //rd.Source = new Uri(@"ExpressionLight.xaml", UriKind.Relative);
            //Application.Current.Resources.MergedDictionaries.Add(rd);
            //Settings.OnUIColorSchemeUpdate += UIColorSchemeChanged;
            Settings.LoadSettings();

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            NewTabButtonVisibility = Visibility.Collapsed;
            
            Authentification.GetAuthentificationTab();
            Tabs = Authentification.Tabs;
            SelectedTabIndex = Authentification.SelectedTabIndex;
            Authentification.AuthentificationComplete += LoadFrameWorkTabs;
            WindowDragAreaWidth = StaticResources.MainWindowWidth - StaticResources.TabTitleDefaultWidth - StaticResources.SystemButtonWidth * 2;
        }

        public async void LoadFrameWorkTabs(AuthentificationEventArgs args)
        {
            if (!args.Result)
                CloseMainWindow();
            Tabs = new ObservableCollection<ClosableTab>();
            Session.UpdateSelectedTab += UpdateSelectedTab;
            Session.UpdateDragAreaWidth += UpdateDragAreaWidth;

            Tabs = await Session.GetSession();
            SelectedTabIndex = Session.SelectedTabIndex;
            NewTabButtonVisibility = Visibility.Visible;
        }

        public async void CloseMainWindow()
        {
            if (Tabs.Count > 0 && Tabs[0].Title.Equals("Authentification"))
            {
                Application.Current.Shutdown();
                return;
            }
            if (await Session.SaveSession())
                Application.Current.Shutdown();
        }

        private void UpdateSelectedTab(EventArgs args)
        {
            SelectedTabIndex = Session.SelectedTabIndex;
        }

        //private void UIColorSchemeChanged(object sender, EventArgs args)
        //{
        //    if (Tabs == null)
        //        return;
        //    if (Tabs.Count == 0)
        //        return;
        //    Session.UpdateTabsHeaderWidth();
        //    Session.UpdateUIWidth(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        //}

        private void UpdateDragAreaWidth(object sender, EventArgs args)
        {
            WindowDragAreaWidth = (args as UpdateDragAreaWidthEventArgs).Width;
        }
    }
}
