using FrameWork.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FrameWork.UC
{
    /// <summary>
    /// Interaction logic for NewPasswordUC1.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for NewPasswordUC.xaml
    /// </summary>
    public partial class NewPasswordView : UserControl
    {
        NewPasswordViewModel viewModel = new NewPasswordViewModel();
        public NewPasswordView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void passwordBoxMain_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.SetNewPassword((sender as PasswordBox).SecurePassword);
            if (passwordBoxRe_enter.Password != string.Empty)
            {
                passwordBoxRe_enter.Password = string.Empty;
            }
        }

        private void passwordBoxRe_enter_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordBoxMain.Password != string.Empty && passwordBoxRe_enter.Password == passwordBoxMain.Password)
            {
                viewModel.ChangePasswordFrame(Colors.LimeGreen);
                viewModel.SubmitButton = true;
            }
            else
            {
                viewModel.ChangePasswordFrame(Colors.Red);
                viewModel.SubmitButton = false;
            }
        }

        private void buttonSubmit_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SubmitNewPassword(passwordBoxMain);
        }
    }
}
