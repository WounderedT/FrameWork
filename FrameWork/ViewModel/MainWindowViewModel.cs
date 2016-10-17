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
        private TabItem _selectedTab;
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
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedTab"));
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

        public MainWindowViewModel()
        {
            App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Authentification.GetAuthentificationTab();
            Tabs = Authentification.Tabs;
            SelectedTab = Authentification.SelectedTab;
            Authentification.AuthentificationComplete += LoadFrameWorkTabs;
        }

        public void LoadFrameWorkTabs(AuthentificationEventArgs args)
        {
            if (!args.Result)
                CloseMainWindow(new CancelEventArgs());
            Session.GetSession();
            Tabs = Session.Tabs;
            SelectedTab = Session.SelectedTab;
            Session.UpdateSelectedTab += UpdateSelectedTab;
        }

        private void UpdateSelectedTab(EventArgs args)
        {
            SelectedTab = Session.SelectedTab;
        }

        public async void CloseMainWindow(CancelEventArgs args)
        {
            if (Tabs.Count == 1)
            {
                App.Current.Shutdown();
                return;
            }
            if (await Session.SaveSession())
                App.Current.Shutdown();
            else
                args.Cancel = true;
        }
    }
}
