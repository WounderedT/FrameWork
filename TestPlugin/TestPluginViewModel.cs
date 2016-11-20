using Interface;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestPlugin
{
    [Serializable]
    public class TestPluginViewModel: INotifyPropertyChanged, ITab
    {
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        [field: NonSerialized()]
        public event EventHandler ProgressReportStart;
        [field: NonSerialized()]
        public event EventHandler ProgressReportUpdate;
        [field: NonSerialized()]
        public event EventHandler ProgressReportComplete;
        [field: NonSerialized()]
        public event Func<object, EventArgs, Task> ReadFromFileRequest;
        [field: NonSerialized()]
        public event Func<object, EventArgs, Task> WriteToFileRequest;

        [NonSerialized]
        private RelayCommand _clickMeButton;
        private string _messageBlock;
        private string _textField;
        private bool _checkBoxState;
        [NonSerialized]
        private bool _isBusy = false;
        [NonSerialized]
        private bool _isChanged = false;
        [NonSerialized]
        private RelayCommand _longRunningActionButton;

        [NonSerialized]
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public CancellationTokenSource CancellationToken
        {
            get { return _cancellationToken; }
        }

        public string Name
        {
            get
            {
                return "TestPLugin";
            }
        }

        public bool isBusy
        {
            get { return _isBusy; }
        }

        public bool isChanged
        {
            get { return _isChanged; }
        }

        public ICommand ClickMeButton
        {
            get
            {
                if(_clickMeButton == null)
                {
                    _clickMeButton = new RelayCommand(param => ButtonClickHandler());
                }
                return _clickMeButton;
            }
        }

        public string TextField
        {
            get { return _textField; }
            set
            {
                if (_textField != value){
                    _textField = value;
                    _isChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextField"));
                }
            }
        }

        public bool CheckBoxState
        {
            get { return _checkBoxState; }
            set
            {
                if(_checkBoxState != value)
                {
                    _checkBoxState = value;
                    _isChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CheckBoxState"));
                }
            }
        }

        public string MessageBlock
        { 
            get{ return _messageBlock; }
            set
            {
                if(_messageBlock != value)
                {
                    _messageBlock = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MessageBlock"));
                }
            }
        }

        public ICommand LongRunningActionButton
        {
            get
            {
                if(_longRunningActionButton == null)
                {
                    _longRunningActionButton = new RelayCommand(async param => await LongRunningAction());
                }
                return _longRunningActionButton;
            }
        }

        public async Task DumpAsync()
        {
            if (_isChanged)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                formatter.Serialize(ms, this);
                FileActionRequestEventArgs args = new FileActionRequestEventArgs(Name + "State", ms.ToArray());
                await OnWriteToFileRequest(args);
            }
        }

        public async Task RestoreAsync()
        {
            FileActionRequestEventArgs args = new FileActionRequestEventArgs(Name + "State");
            await OnReadFromFileRequest(args);
            if (args.Exception != null)
                throw args.Exception;
            if (args.InOutData == null)
                return;
            await Task.Run(() =>
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(args.InOutData, 0, args.InOutData.Length);
                ms.Position = 0;

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new Binder();
                var pluginViewModelState = (TestPluginViewModel)formatter.Deserialize(ms);
                foreach (PropertyInfo property in pluginViewModelState.GetType().GetProperties())
                {
                    try
                    {
                        property.SetValue(this, property.GetValue(pluginViewModelState, null));
                    }
                    catch (ArgumentException) { }
                }
            });
        }

        public async void ButtonClickHandler()
        {
            MessageBlock = "You just pressed the button!";
            FileActionRequestEventArgs args = new FileActionRequestEventArgs("someFile");
            await OnReadFromFileRequest(args);
        }

        public async Task LongRunningAction()
        {
            if(CancellationToken.Token.IsCancellationRequested == true)
                CancellationToken.Token.ThrowIfCancellationRequested();
            _isBusy = true;
            OnProgressReportStart(new EventArgs());
            SynchronizationContext context = SynchronizationContext.Current;
            try
            {
                await Task.Run(async () =>
                    {
                        await Task.Delay(3000, CancellationToken.Token).ConfigureAwait(false);
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            OnProgressReportUpdate(new ProgressReportUpdateEventArgs(0.25));
                        }), null);
                        await Task.Delay(3000, CancellationToken.Token).ConfigureAwait(false);
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            OnProgressReportUpdate(new ProgressReportUpdateEventArgs(0.5));
                        }), null);
                        await Task.Delay(3000, CancellationToken.Token).ConfigureAwait(false);
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            OnProgressReportUpdate(new ProgressReportUpdateEventArgs(0.75));
                        }), null);
                        await Task.Delay(3000, CancellationToken.Token).ConfigureAwait(false);
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            OnProgressReportUpdate(new ProgressReportUpdateEventArgs(1.0));
                        }), null);
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            OnProgressReportCompete(new EventArgs());
                        }), null);
                        return true;
                    }, CancellationToken.Token);
            }
            catch (TaskCanceledException) { }
            _isBusy = false;
        }

        protected virtual void OnProgressReportStart(EventArgs args)
        {
            ProgressReportStart?.Invoke(this, args);
        }

        protected virtual void OnProgressReportUpdate(ProgressReportUpdateEventArgs args)
        {
            ProgressReportUpdate?.Invoke(this, args);
        }

        protected virtual void OnProgressReportCompete(EventArgs args)
        {
            ProgressReportComplete?.Invoke(this, args);
        }

        protected virtual async Task OnReadFromFileRequest(FileActionRequestEventArgs args)
        {
            await ReadFromFileRequest?.Invoke(this, args);
        }

        protected virtual async Task OnWriteToFileRequest(FileActionRequestEventArgs args)
        {
            await WriteToFileRequest?.Invoke(this, args);
        }
    }

    public class Binder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type tyType = null;

            string sShortAssemblyName = assemblyName.Split(',')[0];
            Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly ayAssembly in ayAssemblies)
            {
                if (sShortAssemblyName == ayAssembly.FullName.Split(',')[0])
                {
                    tyType = ayAssembly.GetType(typeName);
                    break;
                }
            }

            return tyType;
        }
    }

    public class ProgressReportUpdateEventArgs : EventArgs, IProgressReportUpdateEventArgs
    {
        public double Value
        {
            get; set;
        }

        public string Status
        {
            get; set;
        }

        public ProgressReportUpdateEventArgs() { }

        public ProgressReportUpdateEventArgs(double value, string status = "OK")
        {
            Value = value;
            Status = status;
        }
    }

    public class FileActionRequestEventArgs : EventArgs, IFileActionRequestEventArgs
    {
        public string FileName { get; set; }
        public byte[] InOutData { get; set; }
        public Exception Exception { get; set; }

        public FileActionRequestEventArgs(string fileName) : this(fileName, null) { }

        public FileActionRequestEventArgs(string fileName, byte[] data)
        {
            FileName = fileName;
            InOutData = data;
        }
    }
}
