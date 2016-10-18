using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FrameWork.ViewModel
{
    class CheckPasswordViewModel: PasswordViewModel, IDataErrorInfo
    {
        public void SubmitPassword(PasswordBox box)
        {
            SecureString str = new SecureString();
            foreach (char c in box.Password)
            {
                str.AppendChar(c);
            }
            if (!Authentification.CheckMasterPassword(str))
            {
                ErrorMessage = Visibility.Visible;
            }
            str.Dispose();
        }
    }
}
