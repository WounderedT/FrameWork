using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool _changePasswordButtonEnable;
        private List<object> _colorThemes;
        private RelayCommand _changePassword;
        private RelayCommand _saveSettings;
        private RelayCommand _cancel;

        private ObservableCollection<UpdatePasswordViewModel> _updatePasswordObject;
        public ObservableCollection<UpdatePasswordViewModel> UpdatePasswordObject
        {
            get { return _updatePasswordObject; }
            set
            {
                if (_updatePasswordObject != value)
                {
                    _updatePasswordObject = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UpdatePasswordObject")); 
                }
            }
        }

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

        public bool ChangePasswordButtonEnable
        {
            get { return _changePasswordButtonEnable; }
            set
            {
                if (_changePasswordButtonEnable != value)
                {
                    _changePasswordButtonEnable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChangePasswordButtonEnable"));
                }
            }
        }

        public List<object> ColorThemes
        {
            get { return _colorThemes; }
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

        public SettingsViewModel()
        {
            ChangePasswordButtonEnable = true;
        }

        private void ChangePassword()
        {
            UpdatePasswordObject = new ObservableCollection<UpdatePasswordViewModel>();
            UpdatePasswordObject.Add(new UpdatePasswordViewModel());
            ChangePasswordButtonEnable = false;
        }

        private void SaveSettings()
        {

        }

        private void Cancel()
        {

        }
    }
}
