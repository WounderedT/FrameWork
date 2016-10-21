using FrameWork.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Visibility NewPassswordErrorFrame
        {
            get { return _newPasswordErrorFrame; }
            set
            {
                if (_newPasswordErrorFrame != value)
                {
                    _newPasswordErrorFrame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewPassswordErrorFrame"));
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

        private void OnSubmitPassword()
        {
            if (!BasePasswordCheck(CheckPassword.Password) || !BasePasswordCheck(NewPassword.Password) || !ValidateNewPasswordReEnter())
                return;
            if (!Authentification.CheckMasterPassword(CheckPassword.Password, true))
            {
                PasswordError = "Incorrect password. Please enter correct password and try again.";
                CheckPasswordErrorFrame = Visibility.Visible;
                return;
            }
            Authentification.NewMasterPassword(NewPassword.Password);
        }

        private void OnCancelPassword()
        {

        }

        private bool ValidateNewPasswordReEnter()
        {
            if (NewPasswordReEnter.Length == 0)
            {
                PasswordError = "Please re-enter new password";
                CheckPasswordErrorFrame = Visibility.Hidden;
                NewPassswordErrorFrame = Visibility.Hidden;
                NewPasswordReEnterErrorFrame = Visibility.Visible;
                return false;
            }
            if (!NewPassword.Equals(NewPasswordReEnter))
            {
                PasswordError = "Entered passwords don't match!";
                CheckPasswordErrorFrame = Visibility.Hidden;
                NewPassswordErrorFrame = Visibility.Visible;
                NewPasswordReEnterErrorFrame = Visibility.Visible;
                return false;
            }
            return true;
        }
    }
}
