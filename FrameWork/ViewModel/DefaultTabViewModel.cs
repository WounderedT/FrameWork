using FrameWork.DataModels;
using Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FrameWork.ViewModel
{
    public class DefaultTabViewModel
    {
        public event GetPluginUIRequestEventHalder GetPluginUIRequest;
        public delegate void GetPluginUIRequestEventHalder(object sender, GetPluginUIRequestEventArgs args);
        public event EventHandler GetSettingsTabRequest;

        public Dictionary<string, PluginEntry> Plugins { get; set; }
        public ObservableCollection<PluginButtonViewModel> PluginButtons { get; set; }

        private RelayCommand _settingsButton;

        public ICommand SettingsButton
        {
            get
            {
                if(_settingsButton == null)
                {
                    _settingsButton = new RelayCommand(param => OnGetSettingsTabRequest(new EventArgs()));
                }
                return _settingsButton;
            }
        }

        public DefaultTabViewModel()
        {
            PluginButtons = new ObservableCollection<PluginButtonViewModel>();
            Plugins = PluginEntryCollection.Plugins;
            GetPluginButtons();
        }

        private void GetPluginButtons()
        {
            foreach (string key in Plugins.Keys)
            {
                PluginButtonViewModel button = new PluginButtonViewModel();
                button.PluginButtonSourceID = Plugins[key].Plugin.Name;
                button.ButtonToolTip = Plugins[key].Plugin.Name;
                button.PluginButtonImage = Plugins[key].PreviewImage;
                button.PluginButtonClick = new RelayCommand(param => EnablePluginUI(Plugins[key].Plugin.Name));
                PluginButtons.Add(button);
            }
        }



        public void EnablePluginUI(string sourcePluginName)
        {
            OnGetPluginUIReques(new GetPluginUIRequestEventArgs(Plugins[sourcePluginName]));
        }

        protected virtual void OnGetPluginUIReques(GetPluginUIRequestEventArgs args)
        {
            GetPluginUIRequest?.Invoke(this, args);
        }

        protected virtual void OnGetSettingsTabRequest(EventArgs args)
        {
            GetSettingsTabRequest?.Invoke(this, args);
        }
    }

    public class GetPluginUIRequestEventArgs: EventArgs
    {
        public PluginEntry SourcePlugin
        {
            get; set;
        }

        public GetPluginUIRequestEventArgs(PluginEntry sourcePlugin)
        {
            SourcePlugin = sourcePlugin;
        }
    }


}
