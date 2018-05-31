using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Downloader.ViewModel
{
    [Serializable]
    public class PatternEntryViewModel: INotifyPropertyChanged
    {
        private List<PatternKeyViewModel> _optionalKeysList;
        private Pattern _pattern;
        private bool _skipMissingFiles;
        private bool _enableZeroPrefix;

        private int _patternFirstIndex;
        private int _patternLastIndex;
        private int _patternFirstIndexOld;
        private int _patternLastIndexOld;
        private int _lastKeyIndex;

        private bool _isDefaultIndexerVisible;

        private string _patternDownloadLink;
        private string _patternDownloadLinkInternal;
        private string _patternGlobalFileName;
        private string _patternFirstIndexText;
        private string _patternLastIndexText;
        private string _patternCustomDownloadFolderName;
        [NonSerialized]
        private string _patternDownloadStatus;
        [NonSerialized]
        private string _patternDownloadStatusToolTip;
        [NonSerialized]
        private int _lastIndMag;

        [NonSerialized]
        private RelayCommand _patternSaveAction;
        [NonSerialized]
        private RelayCommand _patternEditAction;
        [NonSerialized]
        private RelayCommand _patternRemoveAction;
        [NonSerialized]
        private RelayCommand _showDefaultIndexerAction;
        [NonSerialized]
        private RelayCommand _hideDefaultIndexerAction;
        [NonSerialized]
        private RelayCommand _addIndexerAction;
        [NonSerialized]
        private RelayCommand _addNewKeyAction;

        [NonSerialized]
        private Brush _patternDownloadStatusBrush;

        private Visibility _patternCustomDownloadFolderInputVisibility;
        [NonSerialized]
        private Visibility _patternDownloadStatusVisibility = Visibility.Collapsed;

        [NonSerialized]
        private RelayCommand _closePatternEntry;

        [NonSerialized]
        private ObservableCollection<PatternKeyViewModel> _patternKeysList = new ObservableCollection<PatternKeyViewModel>();

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        [field: NonSerialized]
        public event EventHandler CloseEnteryEvent;
        [field: NonSerialized]
        public event EventHandler SavePatternEvent;
        [field: NonSerialized]
        public event EventHandler RemovePatternEvent;

        public string PatternDownloadLinkText
        {
            get { return _patternDownloadLink; }
            set
            {
                if(_patternDownloadLink != value)
                {
                    _patternDownloadLink = value;
                    OnDownloadLinkUpdate();
                    _pattern.IsChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternDownloadLinkText"));
                }
            }
        }

        public string DownloadLinkToolTip
        {
            get
            {
                return "Use " + Pattern.KeySeparator + " symbols to mark the key. Example: "
                    + Pattern.KeySeparator + "_keyName_" + Pattern.KeySeparator;
            }
        }

        public string PatternGlobalFileNameText
        {
            get { return _patternGlobalFileName; }
            set
            {
                if (_patternGlobalFileName != value)
                {
                    _patternGlobalFileName = value;
                    _pattern.IsChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternGlobalFileNameText"));
                }
            }
        }

        public bool SkipMissingFilesCheck
        {
            get { return _skipMissingFiles; }
            set
            {
                if (_skipMissingFiles != value)
                {
                    _skipMissingFiles = value;
                    _pattern.IsChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SkipMissingFilesCheck"));
                }
            }
        }

        public string PatternFirstIndexText
        {
            get { return _patternFirstIndexText; }
            set
            {
                _patternFirstIndexOld = _patternFirstIndex;
                if (_patternFirstIndexText != value && int.TryParse(value, out _patternFirstIndex) && _patternFirstIndex >= 0)
                {
                    if (_patternLastIndex > 0)
                    {
                        if (_patternFirstIndex <= _patternLastIndex)
                        {
                            _patternFirstIndexText = value;
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternFirstIndexText"));
                        }
                        else
                            _patternFirstIndex = _patternFirstIndexOld;
                    }
                    else
                    {
                        _patternFirstIndexText = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternFirstIndexText"));
                    }
                    
                }
            }
        }

        public string PatternLastIndexText
        {
            get { return _patternLastIndexText; }
            set
            {
                _patternLastIndexOld = _patternLastIndex;
                if (_patternLastIndexText != value && int.TryParse(value, out _patternLastIndex) && _patternLastIndex >= _patternFirstIndex)
                {
                    _patternLastIndexText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternLastIndexText"));
                }
                else
                    _patternLastIndex = _patternLastIndexOld;
            }
        }

        public bool EnableZeroPrefixCheck
        {
            get { return _enableZeroPrefix; }
            set
            {
                if (_enableZeroPrefix != value)
                {
                    _enableZeroPrefix = value;
                    _pattern.IsChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableZeroPrefixCheck"));
                }
            }
        }

        public string PatternCustomDownloadFolderNameText
        {
            get { return _patternCustomDownloadFolderName; }
            set
            {
                if (_patternCustomDownloadFolderName != value)
                {
                    _patternCustomDownloadFolderName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternCustomDownloadFolderNameText"));
                }
            }
        }

        public ICommand PatternSaveAction
        {
            get
            {
                if (_patternSaveAction == null)
                {
                    _patternSaveAction = new RelayCommand(param => PatternSave());
                }
                return _patternSaveAction;
            }
        }

        public ICommand PatternEditAction
        {
            get
            {
                if (_patternEditAction == null)
                {
                    _patternEditAction = new RelayCommand(param => PatternEdit());
                }
                return _patternEditAction;
            }
        }

        public ICommand PatternRemoveAction
        {
            get
            {
                if (_patternRemoveAction == null)
                {
                    _patternRemoveAction = new RelayCommand(param => PatternRemove());
                }
                return _patternRemoveAction;
            }
        }

        public ICommand ClosePatternEntry
        {
            get
            {
                if(_closePatternEntry == null)
                {
                    _closePatternEntry = new RelayCommand(param => CloseEntry());
                }
                return _closePatternEntry;
            }
        }

        public ICommand ShowDefaultIndexerAction
        {
            get
            {
                if (_showDefaultIndexerAction == null)
                {
                    _showDefaultIndexerAction = new RelayCommand(param => OnShowIndexerAction());
                }
                return _showDefaultIndexerAction;
            }
        }

        public ICommand HideDefaultIndexerAction
        {
            get
            {
                if (_hideDefaultIndexerAction == null)
                {
                    _hideDefaultIndexerAction = new RelayCommand(param => OnHideIndexerAction());
                }
                return _hideDefaultIndexerAction;
            }
        }

        public ICommand AddIndexerAction
        {
            get
            {
                if (_addIndexerAction == null)
                {
                    _addIndexerAction = new RelayCommand(param => OnAddDefaultIndexer());
                }
                return _addIndexerAction;
            }
        }

        public ICommand AddNewKeyAction
        {
            get
            {
                if (_addNewKeyAction == null)
                {
                    _addNewKeyAction = new RelayCommand(param => OnAddNewKey());
                }
                return _addNewKeyAction;
            }
        }

        public Visibility PatternCustomDownloadFolderInputVisibility
        {
            get { return _patternCustomDownloadFolderInputVisibility; }
            set
            {
                if (_patternCustomDownloadFolderInputVisibility != value)
                {
                    _patternCustomDownloadFolderInputVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternCustomDownloadFolderInputVisibility"));
                }
            }
        }

        public Visibility PatternDownloadStatusVisibility
        {
            get { return _patternDownloadStatusVisibility; }
            set
            {
                if (_patternDownloadStatusVisibility != value)
                {
                    _patternDownloadStatusVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternDownloadStatusVisibility"));
                }
            }
        }

        public Visibility PatternGlobalFileNameVisibility
        {
            get { return _pattern.GlobalFileNameVisibility; }
        }

        public Visibility PatternActonButtonVisibility
        {
            get
            {
                if (_pattern.CanChange)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
        }

        public Visibility PatternSaveButtonVisibility
        {
            get
            {
                if (_pattern.CanSave)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility PatternEditButtonVisibility
        {
            get
            {
                if (_pattern.CanEdit)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility PatternRemoveButtonVisibility
        {
            get
            {
                if (_pattern.CanRemove)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ShowDefaultIndexerVisibility
        {
            get
            {
                if (IsDefaultIndexerVisible)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public Visibility HideDefaultIndexerVisibility
        {
            get
            {
                if (IsDefaultIndexerVisible)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility AddIndexerVisibility
        {
            get
            {
                if (IsDefaultIndexerVisible)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility AddNewKeyVisibility
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public Brush PatternDownloadStatusBrush
        {
            get { return _patternDownloadStatusBrush; }
            set
            {
                if (_patternDownloadStatusBrush != value)
                {
                    _patternDownloadStatusBrush = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternDownloadStatusBrush"));
                }
            }

        }

        public string PatternDownloadStatus
        {
            get { return _patternDownloadStatus; }
            set
            {
                if(_patternDownloadStatus != value)
                {
                    _patternDownloadStatus = value;
                    PatternDownloadStatusToolTip = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternDownloadStatus"));
                }
            }
        }

        public string PatternDownloadStatusToolTip
        {
            get { return _patternDownloadStatusToolTip; }
            set
            {
                if (_patternDownloadStatusToolTip != value)
                {
                    _patternDownloadStatusToolTip = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternDownloadStatusToolTip"));
                }
            }
        }

        private bool IsDefaultIndexerVisible
        {
            get { return _isDefaultIndexerVisible; }
            set
            {
                if(_isDefaultIndexerVisible != value)
                {
                    _isDefaultIndexerVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowDefaultIndexerVisibility"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HideDefaultIndexerVisibility"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AddIndexerVisibility"));
                }
            }
        }

        public int PatternFirstIndex
        {
            get { return _patternFirstIndex; }
        }

        public int PatterLastIndex
        {
            get { return _patternLastIndex; }
        }

        public ObservableCollection<PatternKeyViewModel> PatternKeysList
        {
            get { return _patternKeysList; }
            set { _patternKeysList = value; }
        }

        public PatternEntryViewModel(Pattern pattern, Visibility customDirVisibility = Visibility.Collapsed)
        {
            _pattern = pattern;
            PatternKeysList = new ObservableCollection<PatternKeyViewModel>();
            PatternCustomDownloadFolderInputVisibility = customDirVisibility;
            PatternDownloadStatusVisibility = Visibility.Collapsed;
        }

        public Dictionary<string, string> GetDownloadLinks(string commonDownloadPath)
        {
            Dictionary<string,string> list = new Dictionary<string,string>();
            _patternDownloadLinkInternal = _patternDownloadLink;
            UpdateLastIndexMag();

            var intervalKeys = GetIntervalKeys();
            if (intervalKeys == null)
            {
                return GetDownloadLink(commonDownloadPath);
            }
            //This will only work with one interval.
            foreach(var entry in intervalKeys)
            {
                for (int index = int.Parse(entry.Value[0]); index <= int.Parse(entry.Value[1]); index++)
                {
                    foreach (var result in GetDownloadLink(Path.Combine(commonDownloadPath, index.ToString()), entry.Key, index))
                        list.Add(result.Key, result.Value);
                }
            }
            return list;
        }

        [OnSerializing]
        public void OnSerializing(StreamingContext context)
        {
            if (PatternKeysList == null)
                return;
            _optionalKeysList = PatternKeysList.ToList();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            PatternKeysList = new ObservableCollection<PatternKeyViewModel>();
            foreach (PatternKeyViewModel entry in _optionalKeysList)
                PatternKeysList.Add(entry);
            _optionalKeysList.Clear();
        }

        public void UpdateDownloadStatus(string statusText, string status)
        {
            PatternDownloadStatus = statusText;
            PatternDownloadStatusBrush = GetColor(status);
            PatternDownloadStatusVisibility = Visibility.Visible;
        }

        private Dictionary<string, string> GetDownloadLink(string commonDownloadPath, string intervalKeyName = "", int? intervalIndex = null)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            string extension = GetImageExtension();
            StringBuilder name = new StringBuilder();
            StringBuilder builder = new StringBuilder();

            //if (string.IsNullOrEmpty(_pattern.LinkPattern))
            //    _patternDownloadLinkInternal = Pattern.UpdateLink();

            string downloadPath = (!string.IsNullOrEmpty(_patternCustomDownloadFolderName) && PatternCustomDownloadFolderInputVisibility == Visibility.Visible)
                ? Path.Combine(commonDownloadPath, _patternCustomDownloadFolderName) : commonDownloadPath;
            Directory.CreateDirectory(downloadPath);

            for (int index = _patternFirstIndex; index <= _patternLastIndex; index++)
            {
                name.Clear();
                builder.Clear();
                var link = _patternDownloadLinkInternal;

                foreach(var key in _patternKeysList.Where(k => k.PatternEntryKeyVisibility == Visibility.Visible).Select(k => k))
                {
                    link = link.Replace(key.KeyNameString, key.GetKeyValue());
                }

                link = link.Replace(Pattern.Indexer, GetImageIndex(index));

                list.Add(link, Path.Combine(downloadPath, link.Substring(link.LastIndexOf('/') + 1)));
            }
            return list;
        }

        private string GetImageIndex(int index)
        {
            if (_enableZeroPrefix)
            {
                var num = index.ToString();
                return String.Concat(Enumerable.Repeat("0", _lastIndMag - num.Length)) + num;
            }
            else
            {
                return index.ToString();
            }
        }

        private void UpdateLastIndexMag()
        {
            int res = _patternLastIndex;
            _lastIndMag = 1;
            while (res > 10)
            {
                res /= 10;
                _lastIndMag++;
            }
        }

        private Dictionary<string, string[]> GetIntervalKeys()
        {
            Dictionary<string, string[]> keys = new Dictionary<string, string[]>();
            if (!string.IsNullOrEmpty(_pattern.LinkPattern))
            {
                foreach (PatternKeyViewModel entry in _pattern.Keys)
                {
                    if (entry.KeyValueIsInterval)
                        keys.Add(entry.KeyName, new string[] { entry.KeyValue, entry.KeyIntervalSecondValue });
                }
                return keys;
            }

            if (PatternKeysList.Count == 0)
                return null;

            foreach (PatternKeyViewModel key in PatternKeysList.Where(w => w.PatternEntryKeyVisibility.Equals(Visibility.Visible)
                && w.KeyValueIsInterval))
            {
                keys.Add(key.KeyName, new string[] { key.KeyValue, key.KeyIntervalSecondValue });
            }
            if (keys.Count == 0)
                return null;
            return keys;
        }

        private void PatternKeysParser(ref string[] line)
        {
            if (!string.IsNullOrEmpty(_pattern.LinkPattern))
            {
                foreach (Tuple<int, string> patternEntry in _pattern.GetLinkPatternPositions())
                    line[patternEntry.Item1] = patternEntry.Item2;
                return;
            }

            if (PatternKeysList.Count == 0)
                return;

            foreach (PatternKeyViewModel key in PatternKeysList.Where(w => w.PatternEntryKeyVisibility.Equals(Visibility.Visible)))
            {
                for (int index = 0; index < line.Length; index++)
                    if (line[index].Equals(Pattern.KeySeparator + key.KeyName + Pattern.KeySeparator))
                    {
                        if(!key.KeyValueIsInterval)
                            line[index] = key.KeyValue;
                        else
                            line[index] = "&" + key.KeyName + "_IntervalIndex&";
                        break;
                    }
            }
        }

        private void OnDownloadLinkUpdate()
        {
            IsDefaultIndexerVisible = _patternDownloadLink.Contains(Pattern.Indexer);
            UpdateRequiredKeysList();
        }

        private void MergeLinks()
        {
            if (string.IsNullOrEmpty(_patternDownloadLinkInternal))
            {
                _patternDownloadLinkInternal = _patternDownloadLink;
                return;
            }
            if (_patternDownloadLinkInternal.Equals(_patternDownloadLink))
                return;

            if (Pattern.IndexerRegEx.Matches(_patternDownloadLink).Count == Pattern.IndexerRegEx.Matches(_patternDownloadLinkInternal).Count)
                if (IsIndexerMoved())
                    return;

            StringBuilder builder = new StringBuilder();
            CollapseIndexers();
            char[] charsInt = _patternDownloadLinkInternal.ToCharArray();
            char[] chars = _patternDownloadLink.ToCharArray();
            MergeLinks(ref charsInt, ref chars, linkBuilder: builder);
            _patternDownloadLinkInternal = builder.ToString();
            RestoreIndexers();
        }

        private void MergeLinks(ref char[] internalChars, ref char[] linkChars, StringBuilder linkBuilder = default(StringBuilder))
        {
            Match match = new Match();
            int indexInt = 0;
            int index = 0;
            int builderIndex = 0;
            MatchSequential(ref internalChars, ref linkChars, match, indexInt, index);
            if (match.IsValid)
            {
                linkBuilder.Append(_patternDownloadLinkInternal.Substring(indexInt, match.GetSubstringLength()));
                if (match.Power == linkChars.Length)
                {
                    return;
                }
                else
                {
                    indexInt = match.GetSubstringLength();
                    index = match.GetSubstringLength();
                    builderIndex = match.GetSubstringLength() - match.IndexerDifference;
                }
            }
            List<Match> matchList = new List<Match>();
            for(int ind1 = indexInt; ind1 < internalChars.Length; ind1++)
            {
                for (int ind2 = index; ind2 < linkChars.Length; ind2++)
                {
                    Match matchTmp = new Match();
                    MatchSequential(ref internalChars, ref linkChars, matchTmp, ind1, ind2);
                    if (matchTmp.IsValid)
                        matchList.Add(matchTmp);
                }
            }
            if(matchList.Count > 0)
            {
                List<Match> foundMatches = new List<Match>();
                foundMatches.Add(matchList[0]);
                var lastMatch = matchList[0];
                for (int ind = 1; ind < matchList.Count; ind++)
                {
                    if (lastMatch.PositionInternal < matchList[ind].PositionInternal)
                    {
                        if (lastMatch.Power < (matchList[ind].Power + matchList[ind].PositionInternal - lastMatch.PositionInternal)
                            && lastMatch.IndexerCount < matchList[ind].IndexerCount)
                        {
                            foundMatches.Add(matchList[ind]);
                            lastMatch = matchList[ind];
                        }
                    }
                }
                int diff = 0;
                foreach(var foundMatch in foundMatches)
                {
                    if (foundMatch.Position != builderIndex)
                    {
                        diff = foundMatch.Position - builderIndex;
                        linkBuilder.Append(_patternDownloadLink.Substring(builderIndex, diff));
                        builderIndex += diff;
                    }
                    builderIndex += foundMatch.GetSubstringLength() - foundMatch.IndexerDifference;
                    linkBuilder.Append(_patternDownloadLinkInternal.Substring(foundMatch.PositionInternal, foundMatch.GetSubstringLength()));
                }
            }
            if (builderIndex < linkChars.Length)
                linkBuilder.Append(_patternDownloadLink.Substring(builderIndex, linkChars.Length - builderIndex));
        }

        private void MatchSequential(ref char[] internalChars, ref char[] linkChars, Match match, int positionInt = 0, int position = 0)
        {
            if (positionInt == internalChars.Length || position == linkChars.Length)
                return;

            if (internalChars[positionInt].Equals(Pattern.CollapsedIndexer))
            {
                if (match.HasMatch)
                {
                    match.IndexerCount++;
                    if (linkChars[position].Equals(Pattern.CollapsedIndexer))
                        position++;
                    else
                        match.IndexerDifference++;
                }
                MatchSequential(ref internalChars, ref linkChars, match, ++positionInt, position);
            }
            else
            {
                if (internalChars[positionInt].Equals(linkChars[position]))
                {
                    if (!match.HasMatch)
                    {
                        match.Position = position;
                        match.PositionInternal = positionInt;
                    }
                    match.Power++;
                    MatchSequential(ref internalChars, ref linkChars, match, ++positionInt, ++position);
                }
            }
        }

        private bool IsIndexerMoved()
        {
            if(_patternDownloadLink.Replace(Pattern.Indexer, string.Empty).Equals(_patternDownloadLinkInternal.Replace(Pattern.Indexer, string.Empty)))
            {
                _patternDownloadLinkInternal = _patternDownloadLink;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CollapseIndexers()
        {
            if (_patternDownloadLinkInternal.Contains(Pattern.Indexer))
                _patternDownloadLinkInternal = _patternDownloadLinkInternal.Replace(Pattern.Indexer, Pattern.CollapsedIndexer.ToString());
            if (_patternDownloadLink.Contains(Pattern.Indexer))
                _patternDownloadLink = _patternDownloadLink.Replace(Pattern.Indexer, Pattern.CollapsedIndexer.ToString());
        }

        private void RestoreIndexers()
        {
            if (_patternDownloadLinkInternal.Contains(Pattern.CollapsedIndexer))
                _patternDownloadLinkInternal = _patternDownloadLinkInternal.Replace(Pattern.CollapsedIndexer.ToString(), Pattern.Indexer);
            if (_patternDownloadLink.Contains(Pattern.CollapsedIndexer))
                _patternDownloadLink = _patternDownloadLink.Replace(Pattern.CollapsedIndexer.ToString(), Pattern.Indexer);
        }

        private void UpdateRequiredKeysList()
        {
            string linkTemp = _patternDownloadLink.Replace(Pattern.Indexer, string.Empty);

            if (!Pattern.KeyRegEx.IsMatch(linkTemp))
            {
                foreach (PatternKeyViewModel entry in PatternKeysList)
                    entry.PatternEntryKeyVisibility = Visibility.Collapsed;
                return;
            }
            
            foreach(PatternKeyViewModel entry in PatternKeysList)
            {
                if (linkTemp.Contains(entry.KeyNameString))
                {
                    entry.PatternEntryKeyVisibility = Visibility.Visible;
                    linkTemp = linkTemp.Replace(entry.KeyNameString, string.Empty);
                }
                else
                {
                    entry.PatternEntryKeyVisibility = Visibility.Collapsed;
                }
            }

            foreach(var newEntry in Pattern.KeyRegEx.Matches(linkTemp))
            {
                PatternKeysList.Add(new PatternKeyViewModel(Pattern.GetKeyNameFromKeyString(newEntry.ToString())));
            }
        }

        private string GetIndexValue(int index)
        {
            if (_enableZeroPrefix)
                return string.Concat(Enumerable.Repeat("0", _patternLastIndex.ToString().Length - index.ToString().Length)) + index.ToString();
            else
                return index.ToString();
        }
        
        private string GetImageExtension()
        {
            if (string.IsNullOrEmpty(_pattern.Extension))
                return PatternDownloadLinkText.Substring(PatternDownloadLinkText.LastIndexOf('.'));
            else
                return _pattern.Extension;
        }

        private Brush GetColor(string status)
        {
            switch (status)
            {
                case "OK":
                    return new SolidColorBrush(Colors.LimeGreen);
                case "Error":
                    return new SolidColorBrush(Colors.Red);
                case "Inprogress":
                    return new SolidColorBrush(Colors.DarkBlue);
                case "Pending":
                    return new SolidColorBrush(Colors.Gray);
                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        private void PatternSave()
        {
            OnPatternSave(new EventArgs());
        }

        private void PatternEdit()
        {
            _pattern.GlobalFileNameVisibility = Visibility.Visible;
            if(_pattern.Keys.Count > 0)
            {
                foreach (PatternKeyViewModel entry in _pattern.Keys)
                {
                    entry.PatternEntryKeyVisibility = Visibility.Visible;
                    PatternKeysList.Add(entry);
                }
            }
            _pattern.EditMode = true;
        }

        private void PatternRemove()
        {
            OnPatternRemove(new EventArgs());
        }

        private void CloseEntry()
        {
            OnEntryClose(new EventArgs());
        }

        private void OnShowIndexerAction()
        {
            if (string.IsNullOrEmpty(_patternDownloadLink))
                return;
            IsDefaultIndexerVisible = true;
            MergeLinks();
            if (_patternDownloadLinkInternal.Contains(Pattern.Indexer))
                PatternDownloadLinkText = _patternDownloadLinkInternal;
            else
                OnAddDefaultIndexer();
        }

        private void OnHideIndexerAction()
        {
            IsDefaultIndexerVisible = false;
            _patternDownloadLinkInternal = _patternDownloadLink;
            PatternDownloadLinkText = _patternDownloadLink.Replace(Pattern.Indexer, string.Empty);
        }

        private void OnAddDefaultIndexer()
        {
            PatternDownloadLinkText = InsertToPattern(_patternDownloadLink, Pattern.Indexer);
        }

        private void OnAddNewKey()
        {
            string newKeyName = "KEY" + _lastKeyIndex++;
            PatternKeysList.Add(new PatternKeyViewModel(newKeyName));
            PatternDownloadLinkText = InsertToPattern(_patternDownloadLink, string.Format(Pattern.KeySeparator, newKeyName));
        }

        private string InsertToPattern(string str, string value)
        {
            if(str.Contains("."))
                return str.Insert(str.LastIndexOf('.'), value);
            else
                return str + value;
        }

        protected virtual void OnEntryClose(EventArgs args)
        {
            CloseEnteryEvent?.Invoke(this, args);
        }

        protected virtual void OnPatternSave(EventArgs args)
        {
            SavePatternEvent?.Invoke(this, args);
        }

        protected virtual void OnPatternRemove(EventArgs args)
        {
            RemovePatternEvent?.Invoke(this, args);
        }
    }

    internal class Match
    {
        private const int MinValidPower = 3;
        private int _power;

        public int Power
        {
            get { return _power; }
            set
            {
                _power = value;
                HasMatch = true;
            }
        }
        public int PositionInternal;
        public int Position;
        public int IndexerCount;
        public int IndexerDifference;

        public bool HasMatch { get; private set; }

        public bool IsValid { get { return _power >= MinValidPower; } }

        public int GetSubstringLength()
        {
            return Power + IndexerCount;
        }
    }
}
