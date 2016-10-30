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
        public event PropertyChangedEventHandler PropertyChanged;
        public RelayCommand<DragEventArgs> CloseCommand { get; private set; }

        private ObservableCollection<ClosableTab> _tabs;
        private int _selectedTabIndex = 0;

        private Visibility _newTabButtonVisibility;
        private double _windowDragAreaWidth;

        private RelayCommandAsync _newTabCommand;
        private RelayCommand _minimizeMainWindowCommand;
        private RelayCommand<CancelEventArgs> _closeMainWindowCommand;

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
                if (Tabs.Count == 0)
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
                if (value < 0)
                    _selectedTabIndex = 0;
                else
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
                    _newTabCommand = new RelayCommandAsync(param => Some());
                }
                return _newTabCommand;
            }
        }

        public ICommand CloseMainWindowCommand
        {
            get
            {
                if (_closeMainWindowCommand == null)
                {
                    _closeMainWindowCommand = new RelayCommand<CancelEventArgs>(CloseMainWindow);
                }
                return _closeMainWindowCommand;
            }
        }

        public async Task Some()
        {
            await Session.NewTab();
        }

        public MainWindowViewModel()
        {
            StaticResources.InitializeResources();
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri(@"ExpressionLight.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(rd);
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
                CloseMainWindow(new CancelEventArgs());
            Session.UpdateSelectedTab += UpdateSelectedTab;
            Session.UpdateDragAreaWidth += UpdateDragAreaWidth;

            Tabs = await Session.GetSession();
            SelectedTabIndex = Session.SelectedTabIndex;
            NewTabButtonVisibility = Visibility.Visible;
        }

        public async void CloseMainWindow(CancelEventArgs args)
        {
            if (Tabs.Count == 1)
            {
                Application.Current.Shutdown();
                return;
            }
            if (await Session.SaveSession())
                Application.Current.Shutdown();
            else
                args.Cancel = true;
        }

        private void UpdateSelectedTab(EventArgs args)
        {
            SelectedTabIndex = Session.SelectedTabIndex;
        }

        private void UpdateDragAreaWidth(object sender, EventArgs args)
        {
            WindowDragAreaWidth = (args as UpdateDragAreaWidthEventArgs).Width;
        }

        //private void UpdateUIWidth()
        //{
        //    double minSystemAreaWidth = (double)Application.Current.TryFindResource("SystemButtonHeight")
        //        + (double)Application.Current.TryFindResource("SystemButtonWidth") * 2
        //        + (double)Application.Current.TryFindResource("WindowDragAreaMinWidth");
        //    double tabAreaWidth = (double)Application.Current.TryFindResource("MainWindowWidth") - minSystemAreaWidth;

        //    double width = 0;
        //    foreach(ClosableTab entry in Tabs)
        //    {
        //        width += entry.ActualWidth;
        //    }
        //    if(width > tabAreaWidth)
        //    {
                
        //        width = tabAreaWidth / (Tabs.Count - 1);
        //        foreach(ClosableTab entry in Tabs)
        //        {
        //            entry.HeaderWidth = width - (double)Application.Current.TryFindResource("TabCloseButtonWidth") - entry.GetPaddingWidth();
        //        }
        //    }
        //}
    }
}
