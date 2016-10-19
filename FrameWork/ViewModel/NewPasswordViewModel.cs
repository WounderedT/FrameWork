using FrameWork.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrameWork.ViewModel
{
    class NewPasswordViewModel : PasswordViewModel
    {
        public PasswordString NewPassword { get; set; }
        public PasswordString NewPasswordReEnter { get; set; }
        private Visibility _newPasswordErrorFrame = Visibility.Hidden;
        private Visibility _newPasswordReEnterErrorFrame = Visibility.Hidden;
        private RelayCommand _submitNewPasswod;

        public NewPasswordViewModel()
        {
            NewPassword = new PasswordString();
            NewPasswordReEnter = new PasswordString();
        }

        public Visibility NewPasswordErrorFrame
        {
            get
            {
                return _newPasswordErrorFrame;
            }
            set
            {
                if(_newPasswordErrorFrame != value)
                {
                    _newPasswordErrorFrame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewPasswordErrorFrame"));
                }
            }
        }

        public Visibility NewPasswordReEnterErrorFrame
        {
            get
            {
                return _newPasswordReEnterErrorFrame;
            }
            set
            {
                if (_newPasswordReEnterErrorFrame != value)
                {
                    _newPasswordReEnterErrorFrame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewPasswordReEnterErrorFrame"));
                }
            }
        }

        public ICommand SubmitNewPassword
        {
            get
            {
                if(_submitNewPasswod == null)
                {
                    _submitNewPasswod = new RelayCommand(param => OnSubmitPassword());
                }
                return _submitNewPasswod;
            }
        }

        public void OnSubmitPassword()
        {
            if(ValidateNewPassword() && ValidateNewPasswordReEnter())
            {
                Authentification.NewMasterPassword(NewPassword.Password);
            }
        }

        private bool ValidateNewPassword()
        {
            if(NewPassword.Length == 0)
            {
                PasswordError = "Password cannot be empty!";
                NewPasswordErrorFrame = Visibility.Visible;
                return false;
            }
            return true;
        }

        private bool ValidateNewPasswordReEnter()
        {
            if (NewPasswordReEnter.Length == 0)
            {
                PasswordError = "Please re-enter the password";
                NewPasswordErrorFrame = Visibility.Hidden;
                NewPasswordReEnterErrorFrame = Visibility.Visible;
                return false;
            }
            if (!NewPassword.Equals(NewPasswordReEnter))
            {
                PasswordError = "Passwords don't match!";
                NewPasswordReEnterErrorFrame = Visibility.Visible;
                return false;
            }
            return true;
        }
    }
}
