using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FrameWork.ViewModel
{
    /*TODO:
    1. Update NewPasswordView to work with view model(button state must be changed from view model, not from view!)
    2  Update CheckPasswordView to work with view model(button state must be changed from view model, not from view!)
    3. Update button state as in MVVM pattern example(must be set via property state based on password content check)
    */
    class NewPasswordViewModel: PasswordViewModel
    {
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
