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
    class CheckPasswordViewModel: PasswordViewModel
    {
        private RelayCommand _submitPassword;
        private Visibility _checkPasswordError = Visibility.Hidden;

        public Visibility CheckPasswordError
        {
            get
            {
                return _checkPasswordError;
            }
            set
            {
                if(_checkPasswordError != value)
                {
                    _checkPasswordError = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CheckPasswordError"));
                }
            }
        }

        public ICommand SubmitPassword
        {
            get
            {
                if(_submitPassword == null)
                {
                    _submitPassword = new RelayCommand(param => OnSubmitPassword(param));
                }
                return _submitPassword;
            }
        }

        public void OnSubmitPassword(object box)
        {
            if(!BasePasswordCheck((box as PasswordBox).SecurePassword))
                CheckPasswordError = Visibility.Visible;
            else if (!Authentification.CheckMasterPassword((box as PasswordBox).SecurePassword))
            {
                CheckPasswordError = Visibility.Visible;
            }
        }
    }
}
