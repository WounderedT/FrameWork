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
    }
}
