using FrameWork.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FrameWork.ViewModel
{
    /*TODO:
    1. http://stackoverflow.com/questions/13955013/how-can-i-validate-a-passwordbox-using-idataerrorinfo-without-an-attachedpropert
    2. https://tarundotnet.wordpress.com/2011/03/03/wpf-tutorial-how-to-use-idataerrorinfo-in-wpf/
    */
    class NewPasswordViewModel : PasswordViewModel, IDataErrorInfo, INotifyPropertyChanged
    {
        new private event EventHandler PropertyChanged;
        private PasswordString _newPassword;
        private PasswordString _newPasswordReEnter;

        public void SetNewPassword(SecureString password)
        {
            if (_newPassword == null)
                _newPassword = new PasswordString(password);
            else
                _newPassword.Password = password;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PasswordExtra"));
        }

        public void SetNewPasswordReEnter(SecureString password)
        {
            if (_newPasswordReEnter == null)
                _newPasswordReEnter = new PasswordString(password);
            else
                _newPasswordReEnter.Password = password;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PasswordExtra"));
        }

        public string Error
        {
            get { return string.Empty; }
        }

        public bool PasswordExtra
        {
            get { return false; }
        }

        public string this[string propertyName]
        {
            get
            {
                string errorMessage = string.Empty;
                switch (propertyName)
                {
                    case "PasswordExtra":
                        if (_newPassword == null)
                            return string.Empty;
                        if (_newPassword.Password.Length < 8)
                            return "Password must be at least 8 symbols long!";
                        break;
                }
                return errorMessage;
            }
        }

        public void SubmitNewPassword(PasswordBox box)
        {
            SecureString str = new SecureString();
            foreach (char c in box.Password)
            {
                str.AppendChar(c);
            }
            Authentification.NewMasterPassword(str);
            str.Dispose();
        }
    }
}
