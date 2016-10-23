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

        protected string BasePasswordCheck(SecureString password, string passwordType = null)
        {
            string error = string.Empty;
            if (password.Length == 0)
            {
                if (string.IsNullOrEmpty(passwordType))
                    return "Password cannot be empty!";
                else
                    return passwordType + " password cannot be empty!";
            }
            if(password.Length < _passwordLength)
            {
                if (string.IsNullOrEmpty(passwordType))
                    return "Password must be at least " + _passwordLength + " symbols long!";
                else
                    return passwordType + " password must be at least " + _passwordLength + " symbols long!";
            }
            return null;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
