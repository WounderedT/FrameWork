using System.ComponentModel;
using System.Security;

namespace FrameWork.ViewModel
{
    public class PasswordViewModel: INotifyPropertyChanged
    {
        private const int _passwordLength = 1;
        private const string _passwordTips = "Enter your password here. You may use any password of your choice(yep, even '1' will do the tick).";

        private string _errorMessage = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

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
