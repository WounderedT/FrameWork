using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Downloader.ViewModel
{
    public class AddNewPatternViewModel: INotifyPropertyChanged
    {
        private RelayCommand _newPatternOKButtonAction;
        private RelayCommand _newPatternCancelButtonAction;

        private string _newPatternNameText;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler OKAddNewPatternAction;
        public event EventHandler CancelAddNewPatternAction;

        public ICommand NewPatternOKButtonAction
        {
            get
            {
                if (_newPatternOKButtonAction == null)
                {
                    _newPatternOKButtonAction = new RelayCommand(param => AddNewPattern());
                }
                return _newPatternOKButtonAction;
            }
        }

        public ICommand NewPatternCancelButtonAction
        {
            get
            {
                if (_newPatternCancelButtonAction == null)
                {
                    _newPatternCancelButtonAction = new RelayCommand(param => CancelAddNewPattern());
                }
                return _newPatternCancelButtonAction;
            }
        }

        public string NewPatternNameText
        {
            get { return _newPatternNameText; }
            set
            {
                if (_newPatternNameText != value)
                {
                    _newPatternNameText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewPatternNameText"));
                }
            }
        }

        private void AddNewPattern()
        {
            OKAddNewPatternAction?.Invoke(this, new EventArgs());
        }

        private void CancelAddNewPattern()
        {
            CancelAddNewPatternAction?.Invoke(this, new EventArgs());
        }
    }
}
