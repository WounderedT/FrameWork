using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Downloader.ViewModel
{
    public class DownloaderViewModel: ITab
    { 
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ProgressReportStart;
        public event EventHandler ProgressReportUpdate;
        public event EventHandler ProgressReportComplete;
        public event Func<object, EventArgs, Task> ReadFromFileRequest;
        public event Func<object, EventArgs, Task> WriteToFileRequest;

        private bool _isBusy = false;
        private const byte _threadsCount = 10;

        private RelayCommand _downloadImagesAction;

        private List<string> testList = new List<string>();
        private List<Pattern> Patterns;
        private List<Pattern> CustomPatterns = new List<Pattern>();
        private float _currentProgress = 0;
        private object _lock = new object();

        private Visibility _savePatternWindowVisibility;
        private object _pluginRequestOrigin;

        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public CancellationTokenSource CancellationToken
        {
            get { return _cancellationToken; }
        }

        public string Name
        {
            get
            {
                return "Downloader";
            }
        }

        public bool isBusy
        {
            get { return _isBusy; }
        }

        public bool isChanged
        {
            get
            {
                if (PatternEntries.Count > 1)
                    return true;
                if (PatternEntries.First().PatternEntries.Count > 0)
                    return true;
                return false;
            }
        }

        public ICommand DownloadImagesCommand
        {
            get
            {
                if (_downloadImagesAction == null)
                {
                    _downloadImagesAction = new RelayCommand(async param => await DownloadImagesAsync(), param => CanDownloadImage());
                }
                return _downloadImagesAction;
            }
        }

        public Visibility SavePatternWindowVisibility
        {
            get { return _savePatternWindowVisibility; }
            set
            {
                if(_savePatternWindowVisibility != value)
                {
                    _savePatternWindowVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SavePatternWindowVisibility"));
                }
            }
        }

        public ObservableCollection<AddNewPatternViewModel> SavePatternWindow { get; set; }
        public ObservableCollection<PatternViewModel> PatternEntries { get; set; }

        public async Task DumpAsync()
        {
            if (!isChanged)
                return;

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, CustomPatterns);
            FileActionRequestEventArgs args = new FileActionRequestEventArgs(Name + "CustomPatterns", ms.ToArray());
            await OnWriteToFileRequest(args);

            Dictionary<string, Dictionary<string, object>> currentState = new Dictionary<string, Dictionary<string, object>>();
            foreach (PatternViewModel pattern in PatternEntries)
            {
                Dictionary<string, object> entry = new Dictionary<string, object>();
                entry.Add("PatternCommondDownloadFolderPath", pattern.PatternCommondDownloadFolderPathText);
                entry.Add("PatternEntries", pattern.PatternEntries.ToList());
                currentState.Add(pattern.SelectedPattern, entry);
                               
            }

            formatter = new BinaryFormatter();
            ms = new MemoryStream();
            formatter.Serialize(ms, currentState);
            args = new FileActionRequestEventArgs(Name + "State", ms.ToArray());
            await OnWriteToFileRequest(args);
        }

        public async Task RestoreAsync()
        {
            SavePatternWindow = new ObservableCollection<AddNewPatternViewModel>();
            FileActionRequestEventArgs args = new FileActionRequestEventArgs(Name + "State");
            await OnReadFromFileRequest(args);
            if (args.Exception != null)
                throw args.Exception;

            Patterns = new List<Pattern>();
            Patterns.Add(GetDefaultPattern());
            await GetCustomPatterns();
            Patterns.AddRange(CustomPatterns);

            if (args.InOutData == null)
            {
                PatternEntries = new ObservableCollection<PatternViewModel>();
                PatternEntries.Add(new PatternViewModel(Patterns, this));
                return;
            }
            await Task.Run(() =>
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(args.InOutData, 0, args.InOutData.Length);
                ms.Position = 0;

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new Binder();
                var pluginViewModelState = (Dictionary<string, Dictionary<string, object>>)formatter.Deserialize(ms);
                PatternEntries = new ObservableCollection<PatternViewModel>();

                foreach(string key in pluginViewModelState.Keys)
                {
                    PatternViewModel patternView = new PatternViewModel(Patterns, this);
                    patternView.SelectedPattern = key;
                    patternView.PatternEntries.Clear();
                    patternView.PatternCommondDownloadFolderPathText = (string)pluginViewModelState[key]["PatternCommondDownloadFolderPath"];
                    foreach (PatternEntryViewModel entry in (List<PatternEntryViewModel>)pluginViewModelState[key]["PatternEntries"])
                    {
                        entry.PatternDownloadStatusVisibility = Visibility.Collapsed;
                        patternView.AddPatternEntry(entry);
                    }
                    PatternEntries.Add(patternView);
                }
            });
        }

        public async Task DownloadImagesAsync()
        {
            if (CancellationToken.Token.IsCancellationRequested == true)
                CancellationToken.Token.ThrowIfCancellationRequested();
            _isBusy = true;
            OnProgressReportStart(new EventArgs());
            SynchronizationContext context = SynchronizationContext.Current;
            float progressBarStep = CalculateProgresBarStep();
            List<Task> downloads = new List<Task>();
            try
            {
                foreach (PatternViewModel pattern in PatternEntries)
                    foreach (PatternEntryViewModel entry in pattern.PatternEntries)
                        entry.UpdateDownloadStatus(DownloadStatus.Pending);
                foreach (PatternViewModel pattern in PatternEntries)
                {
                    pattern.PatterViewIsEnabled = false;
                    foreach(PatternEntryViewModel entry in pattern.PatternEntries)
                    {
                        bool canceled = false;
                        entry.UpdateDownloadStatus(DownloadStatus.Inprogress);
                        var links = entry.GetDownloadLinks(pattern.PatternCommondDownloadFolderPathText);
                        downloads.Clear();
                        while(downloads.Count <= _threadsCount && links.Count > 0)
                        {
                            downloads.Add(SimpleDownload(links.First().Key, links.First().Value, progressBarStep, context));
                            links.Remove(links.First().Key);
                        }
                        while (downloads.Count > 0)
                        {
                            Task complete = await Task.WhenAny(downloads);
                            if(complete.Exception != null)
                            {
                                if (complete.Exception.InnerException.Message.Equals("The remote server returned an error: (404) Not Found."))
                                {
                                    if (!entry.SkipMissingFilesCheck)
                                    {
                                        entry.UpdateDownloadStatus(DownloadStatus.Error);
                                        entry.PatternDownloadStatusToolTip = complete.Exception.InnerException.Message;
                                        canceled = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    entry.UpdateDownloadStatus(DownloadStatus.Error);
                                    entry.PatternDownloadStatusToolTip = complete.Exception.InnerException.Message;
                                    canceled = true;
                                    break;
                                }  
                            }
                            downloads.Remove(complete);

                            if(links.Count > 0)
                            {
                                downloads.Add(SimpleDownload(links.First().Key, links.First().Value, progressBarStep, context));
                                links.Remove(links.First().Key);
                            }
                        }
                        if(!canceled)
                            entry.UpdateDownloadStatus(DownloadStatus.Error);
                    }
                    pattern.PatterViewIsEnabled = true;
                }
                context.Post(new SendOrPostCallback((o) =>
                {
                    OnProgressReportUpdate(new ProgressReportUpdateEventArgs(1.0));
                }), null);
                context.Post(new SendOrPostCallback((o) =>
                {
                    OnProgressReportCompete(new EventArgs());
                }), null);
                _isBusy = false;
            }
            catch (TaskCanceledException) { }
            _isBusy = false;
        }

        public void OnSavePatternEvent(object sender, EventArgs args)
        {
            var confirmationUC = new AddNewPatternViewModel();
            confirmationUC.OKAddNewPatternAction += SaveNewPattern;
            confirmationUC.OKAddNewPatternAction += CancelNewPattern;
            SavePatternWindow.Add(confirmationUC);
            _pluginRequestOrigin = sender;
        }

        public void OnRemovePatternEvent(object sender, EventArgs args)
        {
            //this must be changed if more than one active patter could be used simultaneously.
            Pattern removePattern = CustomPatterns.Where(w => w.Name.Equals(PatternEntries.First().SelectedPattern)).FirstOrDefault();
            CustomPatterns.Remove(removePattern);

            foreach (PatternViewModel entry in PatternEntries)
            {
                entry.AvailablePatterns.Remove(removePattern.Name);
                PatternEntries.First().RemovePattern(removePattern);
            }

            PatternEntries.First().SelectedPattern = "Default";
        }

        public void SaveNewPattern(object sender, EventArgs args)
        {
            Pattern newPattern = new Pattern();
            newPattern.Name = (sender as AddNewPatternViewModel).NewPatternNameText;
            newPattern.Parse(_pluginRequestOrigin as PatternEntryViewModel);
            CustomPatterns.Add(newPattern);
            foreach (PatternViewModel entry in PatternEntries)
            {
                entry.AvailablePatterns.Add(newPattern.Name);
                PatternEntries.First().AddPattern(newPattern);
            }
            
            //this must be changed if more than one active patter could be used simultaneously.
            PatternEntries.First().SelectedPattern = newPattern.Name;
            SavePatternWindow.Clear();
        }

        private void CancelNewPattern(object sender, EventArgs args)
        {
            SavePatternWindow.Clear();
        }

        private async Task GetCustomPatterns()
        {
            FileActionRequestEventArgs args = new FileActionRequestEventArgs(Name + "CustomPatterns");
            await OnReadFromFileRequest(args);
            if (args.Exception != null)
                throw args.Exception;
            if (args.InOutData != null)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(args.InOutData, 0, args.InOutData.Length);
                ms.Position = 0;

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new Binder();
                CustomPatterns = (List<Pattern>)formatter.Deserialize(ms);
            }
        }

        private async Task SimpleDownload(string link, string fileName, float step, SynchronizationContext context)
        {
            using (var client = new System.Net.WebClient())
            {
                Task task = client.DownloadFileTaskAsync(link, fileName);
                await task;
                if(task.Exception != null)
                    throw task.Exception.InnerException;
                _currentProgress += step;
                context.Post(new SendOrPostCallback((o) =>
                {
                    OnProgressReportUpdate(new ProgressReportUpdateEventArgs(_currentProgress));
                }), null);
            }
        }

        private bool CanDownloadImage()
        {
            return !isBusy;
        }

        private Pattern GetDefaultPattern()
        {
            Pattern pattern = new Pattern(true);
            pattern.GlobalFileNameVisibility = Visibility.Visible;
            return pattern;
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

        private float CalculateProgresBarStep()
        {
            int totalImagesNumber = 0;
            foreach (PatternViewModel pattern in PatternEntries)
            {
                foreach (PatternEntryViewModel entry in pattern.PatternEntries)
                {
                    totalImagesNumber += entry.PatterLastIndex - entry.PatternFirstIndex + 1;
                }
            }
            float step = 1f / totalImagesNumber;
            _currentProgress = 1 - totalImagesNumber * step;
            return step;
        }
    }

    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
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
            
            if (tyType == null && typeName.Contains("[["))
            {
                if (typeName.Contains("System.Collections.Generic.List`1[[Downloader.Pattern, Downloader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"))
                    tyType = typeof(List<Pattern>);
                if (typeName.Contains("System.Collections.Generic.List`1[[Downloader.ViewModel.PatternEntryViewModel, Downloader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"))
                    tyType = typeof(List<PatternEntryViewModel>);
                if (typeName.Contains("System.Collections.Generic.List`1[[Downloader.ViewModel.PatternKeyViewModel, Downloader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"))
                    tyType = typeof(List<PatternKeyViewModel>);
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
