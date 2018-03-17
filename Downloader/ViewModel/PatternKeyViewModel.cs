using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Downloader.ViewModel
{
    [Serializable]
    public class PatternKeyViewModel: INotifyPropertyChanged
    {
        private string _keyName;
        private string _keyValueFromStr;
        private string _keyValueToStr;
        private string _keyValueStepStr;
        private int _keyValueFrom;
        private int _keyValueTo;
        private int _keyValueStep;
        private bool _keyValueIsInterval;
        private Visibility _patternEntryKeyVisibility;

        [NonSerialized]
        private int _counter;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public string KeyName
        {
            get
            {
                return _keyName;
            }
            set
            {
                if(_keyName != value)
                {
                    _keyName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("KeyName"));
                }
            }
        }
        
        public string KeyNameString
        {
            get { return string.Format(Pattern.KeySeparator, _keyName); }
        }

        public string KeyValue
        {
            get
            {
                return _keyValueFromStr;
            }
            set
            {
                if (_keyValueFromStr != value && int.TryParse(value, out _keyValueFrom))
                {
                    _keyValueFromStr = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("KeyValue"));
                }
            }
        }

        public string KeyIntervalSecondValue
        {
            get
            {
                return _keyValueToStr;
            }
            set
            {
                if (_keyValueToStr != value && int.TryParse(value, out _keyValueTo))
                {
                    _keyValueToStr = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("KeyIntervalSecondValue"));
                }
            }
        }

        public string KeyValueStep
        {
            get
            {
                return _keyValueStepStr;
            }
            set
            {
                if (_keyValueStepStr != value && int.TryParse(value, out _keyValueStep))
                {
                    _keyValueStepStr = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("KeyValueStep"));
                }
            }
        }

        public string KeyValueToolTip
        {
            get
            {
                return"Special key values:" + Environment.NewLine
                    + "&IterationIndex& - key will be replaced by current iteration index";
            }
        }

        public bool KeyValueIsInterval
        {
            get { return _keyValueIsInterval; }
            set
            {
                if(_keyValueIsInterval != value)
                {
                    _keyValueIsInterval = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("KeyValueIsInterval"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SecondKeyVisibility"));
                }
            }
        }

        public Visibility PatternEntryKeyVisibility
        {
            get { return _patternEntryKeyVisibility; }
            set
            {
                if(_patternEntryKeyVisibility != value)
                {
                    _patternEntryKeyVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternEntryKeyVisibility"));
                }
            }
        }

        public Visibility SecondKeyVisibility
        {
            get
            {
                if (_keyValueIsInterval) { return Visibility.Visible; }
                else { return Visibility.Collapsed; }
            }
        }

        public PatternKeyViewModel(string keyName)
        {
            KeyName = keyName;
            PatternEntryKeyVisibility = Visibility.Visible;
        }

        public string GetKeyValue()
        {
            if (_keyValueStep == 0)
                _keyValueStep = 1;
            return (_keyValueFrom + _keyValueStep * ++_counter).ToString();
        }
    }
}
