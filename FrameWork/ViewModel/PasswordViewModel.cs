using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FrameWork.ViewModel
{
    public class PasswordViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Brush _passwordFrame;
        private bool _submitButton = false;
        private string _errorMessage = "Hidden";

        private string _passwordTips = "Enter your password here. You may use any password of your choice(yep, even '1' will do the tick).";

        public string PasswordTips
        {
            get { return _passwordTips; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ErrorMessage"));
                }
            }
        }

        public Brush PasswordFrame
        {
            get
            {
                return _passwordFrame;
            }
            set
            {
                if (_passwordFrame != value)
                {
                    _passwordFrame = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PasswordFrame"));
                }
            }
        }

        public bool SubmitButton
        {
            get { return _submitButton; }
            set
            {
                if(_submitButton != value)
                {
                    _submitButton = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SubmitButton"));
                }
            }
        }

        public void ChangePasswordFrame(Color color)
        {
            SolidColorBrush scb = new SolidColorBrush();
            scb.Color = color;
            PasswordFrame = scb;
        }
    }
}
