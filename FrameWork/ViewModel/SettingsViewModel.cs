using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FrameWork.ViewModel
{
    public class SettingsViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _encryptFiles;
        private RelayCommand _changePassword;
        private List<object> _colorThemes;
        private RelayCommand _saveSettings;
        private RelayCommand _cancel;

        public bool EncryptFiles
        {
            get { return _encryptFiles; }
            set
            {
                if(_encryptFiles != value)
                {
                    _encryptFiles = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EncryptFiles"));
                }
            }
        }

        public ICommand ChangePasswordButton
        {
            get
            {
                if(_changePassword == null)
                {
                    _changePassword = new RelayCommand(param => ChangePassword());
                }
                return _changePassword;
            }
        }

        public List<object> ColorThemes
        {
            get { return _colorThemes; }
        }

        public ICommand SaveSettingsButton
        {
            get
            {
                if(_saveSettings == null)
                {
                    _saveSettings = new RelayCommand(param => SaveSettings());
                }
                return _saveSettings;
            }
        }

        public ICommand CancelButton
        {
            get
            {
                if(_cancel == null)
                {
                    _cancel = new RelayCommand(param => Cancel());
                }
                return _cancel;
            }
        }

        private void ChangePassword()
        {

        }

        private void SaveSettings()
        {

        }

        private void Cancel()
        {

        }
    }
}
