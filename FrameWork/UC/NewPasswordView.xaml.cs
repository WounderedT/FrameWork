using FrameWork.ViewModel;
using System.Windows;
using System.Windows.Controls;

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
            viewModel.NewPassword.Password = (sender as PasswordBox).SecurePassword;
        }

        private void passwordBoxRe_enter_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.NewPasswordReEnter.Password = (sender as PasswordBox).SecurePassword;
        }
    }
}
