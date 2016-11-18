using FrameWork.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace FrameWork.UC
{
    /// <summary>
    /// Interaction logic for UpdatePasswordView.xaml
    /// </summary>
    public partial class UpdatePasswordView : UserControl
    {
        public UpdatePasswordViewModel viewModel = new UpdatePasswordViewModel();
        public UpdatePasswordView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void checkPasswordPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.CheckPassword.Password = (sender as PasswordBox).SecurePassword;
        }

        private void newPasswordPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.NewPassword.Password = (sender as PasswordBox).SecurePassword;
        }

        private void reEnterPasswordPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.NewPasswordReEnter.Password = (sender as PasswordBox).SecurePassword;
        }
    }
}
