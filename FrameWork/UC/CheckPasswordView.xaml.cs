using FrameWork.ViewModel;
using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace FrameWork.UC
{
    /// <summary>
    /// Interaction logic for EnterPasswordUC.xaml
    /// </summary>
    public partial class CheckPasswordView : UserControl
    {
        CheckPasswordViewModel viewModel = new CheckPasswordViewModel();
        public CheckPasswordView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void passwordBoxMain_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordBoxMain.Password != string.Empty)
            {
                viewModel.SubmitButton = true;
            }
            else
            {
                viewModel.SubmitButton = false;
            }
        }

        private void buttonSubmit_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SubmitPassword(passwordBoxMain);
        }
    }
}
