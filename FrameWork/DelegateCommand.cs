using System;
using System.Diagnostics;
using System.Windows.Input;

namespace FrameWork
{
    public class DelegateCommand: ICommand
    {
        public event EventHandler CanExecuteChanged;

        Action<object> _delegate;

        public DelegateCommand(Action<object> action)
        {
            _delegate = action;
        }

        public void Execute(object parameter)
        {
            _delegate(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
