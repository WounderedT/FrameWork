using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FrameWork.ViewModel
{
    public class PasswordViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const int _passwordLength = 1;
        private string _errorMessage = string.Empty;

        private string _passwordTips = "Enter your password here. You may use any password of your choice(yep, even '1' will do the tick).";

        public string PasswordTips
        {
            get { return _passwordTips; }
        }

        public string PasswordError
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("PasswordError"));
                }
            }
        }

        protected bool BasePasswordCheck(SecureString password, string passwordType = "")
        {
            if (password.Length == 0)
            {
                if (string.IsNullOrEmpty(passwordType))
                    PasswordError = "Password cannot be empty!";
                else
                    PasswordError = passwordType + " password cannot be empty!";
                return false;
            }
            if(password.Length < _passwordLength)
            {
                if (string.IsNullOrEmpty(passwordType))
                    PasswordError = "Password must be at least " + _passwordLength + " symbols long!";
                else
                    PasswordError = passwordType + " password must be at least " + _passwordLength + " symbols long!";
                return false;
            }
            return true;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
