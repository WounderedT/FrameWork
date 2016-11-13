using FrameWork.DataModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FrameWork.ViewModel
{
    public class UpdatePasswordViewModel: PasswordViewModel
    {
        private Visibility _checkPasswordErrorFrame = Visibility.Hidden;
        private Visibility _newPasswordErrorFrame = Visibility.Hidden;
        private Visibility _newPasswordReEnterErrorFrame = Visibility.Hidden;
        private RelayCommand _submitPassword;
        private RelayCommand _cancelPassword;
        private string _checkPasswordError = string.Empty;
        private string _newPasswordError = string.Empty;
        private string _newPasswordReEnterError = string.Empty;

        public event EventHandler passwordUpdateComplete;
        public PasswordString CheckPassword { get; set; }
        public PasswordString NewPassword { get; set; }
        public PasswordString NewPasswordReEnter { get; set; }

        public Visibility CheckPasswordErrorFrame
        {
            get { return _checkPasswordErrorFrame; }
            set
            {
                if(_checkPasswordErrorFrame != value)
                {
                    _checkPasswordErrorFrame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CheckPasswordErrorFrame"));
                }
            }
        }

        public Visibility NewPasswordErrorFrame
        {
            get { return _newPasswordErrorFrame; }
            set
            {
                if (_newPasswordErrorFrame != value)
                {
                    _newPasswordErrorFrame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewPasswordErrorFrame"));
                }
            }
        }

        public Visibility NewPasswordReEnterErrorFrame
        {
            get { return _newPasswordReEnterErrorFrame; }
            set
            {
                if (_newPasswordReEnterErrorFrame != value)
                {
                    _newPasswordReEnterErrorFrame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewPasswordReEnterErrorFrame"));
                }
            }
        }

        public ICommand SubmitPassword
        {
            get
            {
                if(_submitPassword == null)
                {
                    _submitPassword = new RelayCommand(param => OnSubmitPassword());
                }
                return _submitPassword;
            }
        }

        public ICommand CancelPassword
        {
            get
            {
                if(_cancelPassword == null)
                {
                    _cancelPassword = new RelayCommand(param => OnCancelPassword());
                }
                return _cancelPassword;
            }
        }

        public string CheckPasswordError
        {
            get { return _checkPasswordError; }
            set
            {
                if(_checkPasswordError != value)
                {
                    _checkPasswordError = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CheckPasswordError"));
                }
            }
        }

        public string NewPasswordError
        {
            get { return _newPasswordError; }
            set
            {
                if (_newPasswordError != value)
                {
                    _newPasswordError = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewPasswordError"));
                }
            }
        }

        public string NewPasswordReEnterError
        {
            get { return _newPasswordReEnterError; }
            set
            {
                if (_newPasswordReEnterError != value)
                {
                    _newPasswordReEnterError = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewPasswordReEnterError"));
                }
            }
        }

        public bool isChanged
        {
            get
            {
                return !(CheckPassword.isEmpty && NewPassword.isEmpty && NewPasswordReEnter.isEmpty);
            }
        }

        public UpdatePasswordViewModel()
        {
            CheckPassword = new PasswordString();
            NewPassword = new PasswordString();
            NewPasswordReEnter = new PasswordString();
        }

        public bool OnSubmitPassword()
        {
            ClearValidationErros();
            string result = BasePasswordCheck(CheckPassword.Password, CheckPasswordError);
            if(!string.IsNullOrEmpty(result))
            {
                CheckPasswordError = result;
                CheckPasswordErrorFrame = Visibility.Visible;
                return false;
            }
            result = BasePasswordCheck(NewPassword.Password, CheckPasswordError);
            if (!string.IsNullOrEmpty(result))
            {
                NewPasswordError = result;
                NewPasswordErrorFrame = Visibility.Visible;
                return false;
            }
            if (CheckPassword.Equals(NewPassword))
            {
                NewPasswordError = "Cannot use current password as a new one!";
                NewPasswordErrorFrame = Visibility.Visible;
                return false;
            }
            if (!ValidateNewPasswordReEnter())
                return false;
            if (!Authentification.CheckMasterPassword(CheckPassword.Password, true))
            {
                CheckPasswordError = "Incorrect password!";
                CheckPasswordErrorFrame = Visibility.Visible;
                return false;
            }
            Authentification.NewMasterPassword(NewPassword.Password, true);
            OnPasswordUpdateComplete();
            return true;
        }

        private void OnCancelPassword()
        {
            OnPasswordUpdateComplete();
        }

        private bool ValidateNewPasswordReEnter()
        {
            if (NewPasswordReEnter.Length == 0)
            {
                NewPasswordReEnterError = "Please re-enter new password";
                CheckPasswordErrorFrame = Visibility.Hidden;
                NewPasswordErrorFrame = Visibility.Hidden;
                NewPasswordReEnterErrorFrame = Visibility.Visible;
                return false;
            }
            if (!NewPassword.Equals(NewPasswordReEnter))
            {
                NewPasswordReEnterError = "Passwords don't match!";
                NewPasswordError = "Passwords don't match!";
                CheckPasswordErrorFrame = Visibility.Hidden;
                NewPasswordErrorFrame = Visibility.Visible;
                NewPasswordReEnterErrorFrame = Visibility.Visible;
                return false;
            }
            return true;
        }

        private void ClearValidationErros()
        {
            CheckPasswordError = string.Empty;
            CheckPasswordErrorFrame = Visibility.Hidden;
            NewPasswordError = string.Empty;
            NewPasswordErrorFrame = Visibility.Hidden;
            NewPasswordReEnterError = string.Empty;
            NewPasswordReEnterErrorFrame = Visibility.Hidden;
        }

        protected virtual void OnPasswordUpdateComplete()
        {
            passwordUpdateComplete?.Invoke(this, new EventArgs());
        }
    }
}
