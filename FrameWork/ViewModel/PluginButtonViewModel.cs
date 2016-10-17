using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace FrameWork.ViewModel
{
    public class PluginButtonViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Visibility _visibilityPluginButtonFrame;
        private BitmapSource _pluginButtonImage;
        private RelayCommand _pluginButtonClick;
        private string _pluginName;
        private string _buttonToolTip;

        public string PluginButtonSourceID
        {
            get { return _pluginName; }
            set
            {
                if (_pluginName != value)
                {
                    _pluginName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PluginButtonSourceID"));
                }
            }
        }

        public ICommand PluginButtonClick
        {
            get
            {
                return _pluginButtonClick;
            }
            set
            {
                _pluginButtonClick = (RelayCommand)value;
            }
        }

        public Visibility VisibilityPluginButtonFrame
        {
            get { return _visibilityPluginButtonFrame; }
            set
            {
                if(_visibilityPluginButtonFrame != value)
                {
                    _visibilityPluginButtonFrame = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisibilityPluginButtonFrame"));
                }
            }
        }

        public BitmapSource PluginButtonImage
        {
            get { return _pluginButtonImage; }
            set
            {
                if (_pluginButtonImage != value)
                {
                    _pluginButtonImage = value;  
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PluginButtonImage"));
                }
            }
        }

        public string ButtonToolTip
        {
            get { return _buttonToolTip; }
            set
            {
                if (_buttonToolTip != value)
                {
                    _buttonToolTip = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ButtonToolTip"));
                }
            }
        }
    }
}
