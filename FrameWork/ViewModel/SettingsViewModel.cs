using FrameWork.UC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace FrameWork.ViewModel
{
    public class SettingsViewModel: INotifyPropertyChanged
    {
        private bool _encryptFiles;
        private bool _changePasswordButtonEnable;
        private List<string> _colorSchemes;
        private RelayCommand _changePassword;
        private RelayCommand _saveSettings;
        private RelayCommand _cancel;
        private ObservableCollection<UpdatePasswordView> _updatePasswordObject;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<UpdatePasswordView> UpdatePasswordObject
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

        public bool EnableEncryption
        {
            get { return _encryptFiles; }
            set
            {
                if(_encryptFiles != value)
                {
                    _encryptFiles = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableEncryption"));
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

        public string SelectedColorScheme { get; set; }

        public List<string> ColorSchemes
        {
            get { return _colorSchemes; }
            set { _colorSchemes = value; }
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
            EnableEncryption = Settings.EncryptFiles;
            ColorSchemes = Settings.ColorSchemes;
            SelectedColorScheme = Settings.CurrentColorScheme;
        }

        private void ChangePassword()
        {
            UpdatePasswordObject = new ObservableCollection<UpdatePasswordView>();
            UpdatePasswordView view = new UpdatePasswordView();
            view.viewModel.passwordUpdateComplete += PasswordUpdateComplete;
            UpdatePasswordObject.Add(view);
            ChangePasswordButtonEnable = false;
        }

        private void PasswordUpdateComplete(object sender, EventArgs args)
        {
            UpdatePasswordObject.RemoveAt(0);
            ChangePasswordButtonEnable = true;
        }

        private void SaveSettings()
        {
            if (UpdatePasswordObject != null && UpdatePasswordObject.Count > 0)
                if (UpdatePasswordObject[0].viewModel.isChanged)
                    if (!UpdatePasswordObject[0].viewModel.OnSubmitPassword())
                        return;
            Settings.EncryptFiles = EnableEncryption;
            Settings.CurrentColorScheme = SelectedColorScheme;
            Settings.SaveSettings();
            Session.CloseTab();
        }

        private void Cancel()
        {
            var some = SelectedColorScheme;
            Session.CloseTab();
        }
    }
}
