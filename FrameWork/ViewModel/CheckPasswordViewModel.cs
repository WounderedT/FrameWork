using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FrameWork.ViewModel
{
    class CheckPasswordViewModel: PasswordViewModel
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
                ErrorMessage = "Visible";
            }
            str.Dispose();
        }
    }
}
