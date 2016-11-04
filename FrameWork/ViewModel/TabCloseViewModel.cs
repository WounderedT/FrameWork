using System;
using System.Collections.Generic;
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
    public class TabCloseViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private object _labelContent;
        private Brush _labelBackground;
        private Visibility _buttonCloseVisibility;
        private RelayCommand _closeButtonClick;
        private double _closableTabLabelWidth = StaticResources.TabTitleDefaultWidth;
        private double _tabCloseButtonWidth = StaticResources.TabCloseButtonWidth;

        public object LabelContent
        {
            get { return _labelContent; }
            set
            {
                if(_labelContent != value)
                {
                    _labelContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LabelContent"));
                }
            }
        }
        
        public Brush LabelBackground
        {
            get { return _labelBackground; }
            set
            {
                if (_labelBackground != value)
                {
                    _labelBackground = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LabelBackground"));
                }
            }
        }

        public double ClosableTabLabelWidth
        {
            get { return _closableTabLabelWidth; }
            set
            {
                if (_closableTabLabelWidth != value)
                {
                    _closableTabLabelWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClosableTabLabelWidth"));
                }
            }
        }

        public double TabCloseButtonWidth
        {
            get { return _tabCloseButtonWidth; }
            set
            {
                if (_tabCloseButtonWidth != value)
                {
                    _tabCloseButtonWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TabCloseButtonWidth"));
                }
            }
        }

        public ICommand CloseButtonClick
        {
            get
            {
                return _closeButtonClick;
            }
            set
            {
                _closeButtonClick = (RelayCommand)value;
            }
        }

        public Visibility ButtonCloseVisibility
        {
            get { return _buttonCloseVisibility; }
            set
            {
                if (_buttonCloseVisibility != value)
                {
                    _buttonCloseVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ButtonCloseVisibility"));
                }
            }
        }
    }
}
