﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Downloader.ViewModel
{
    public class PatternViewModel: INotifyPropertyChanged
    {
        private List<Pattern> _patterns;
        private string _selectedPattern;
        private string _patternCommondDownloadFolderPathText;
        private RelayCommand _addPatternEntryInstance;
        private RelayCommand _openFileBrowser;

        private readonly DownloaderViewModel _sender;

        private bool _patternViewIsEnabled = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool PatterViewIsEnabled
        {
            get { return _patternViewIsEnabled; }
            set
            {
                if(_patternViewIsEnabled != value)
                {
                    _patternViewIsEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatterViewIsEnabled"));
                }
            }
        }

        public ICommand AddPatternEntryInstance
        {
            get
            {
                if(_addPatternEntryInstance == null)
                {
                    _addPatternEntryInstance = new RelayCommand(param => AddPatternEntry());
                }
                return _addPatternEntryInstance;
            }
        }

        public ICommand OpenFileBrowser
        {
            get
            {
                if(_openFileBrowser == null)
                {
                    _openFileBrowser = new RelayCommand(param => SelectDownloadPath());
                }
                return _openFileBrowser;
            }
        }

        public string SelectedPattern
        {
            get { return _selectedPattern; }
            set
            {
                if(_selectedPattern != value)
                {
                    _selectedPattern = value;
                    PatternEntries.Clear();
                    AddPatternEntry();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedPattern"));
                }
            }
        }
        public ObservableCollection<PatternEntryViewModel> PatternEntries { get; set; }
        public ObservableCollection<string> AvailablePatterns { get; set; }

        public string PatternCommondDownloadFolderPathText
        {
            get { return _patternCommondDownloadFolderPathText; }
            set
            {
                if(_patternCommondDownloadFolderPathText != value)
                {
                    _patternCommondDownloadFolderPathText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PatternCommondDownloadFolderPathText"));
                }
            }
        }

        public PatternViewModel(List<Pattern> patterns, DownloaderViewModel sender)
        {
            _sender = sender;
            PatternEntries = new ObservableCollection<PatternEntryViewModel>();
            PatternEntries.CollectionChanged += OnPatternEtriesCollectionChanged;
            AvailablePatterns = new ObservableCollection<string>();
            _patterns = patterns;
            foreach(Pattern key in _patterns)
                AvailablePatterns.Add(key.Name);
        }

        public void AddPattern(Pattern pattern)
        {
            _patterns.Add(pattern);
        }

        public void RemovePattern(Pattern pattern)
        {
            _patterns.Remove(pattern);
        }

        public void AddPatternEntry(PatternEntryViewModel entry = null)
        {
            Visibility customDirVisibility = Visibility.Collapsed;
            if (PatternEntries.Count > 0)
                customDirVisibility = Visibility.Visible;
            if(entry == null)
                entry = new PatternEntryViewModel(GetSelectedPattern(_selectedPattern), customDirVisibility);
            entry.CloseEnteryEvent += ClosePatternEntry;
            entry.SavePatternEvent += _sender.OnSavePatternEvent;
            entry.RemovePatternEvent += _sender.OnRemovePatternEvent;
            PatternEntries.Add(entry);
        }

        private Pattern GetSelectedPattern(string name)
        {
            return _patterns.Where(w => w.Name.Equals(name)).FirstOrDefault();
        }

        private void SelectDownloadPath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select Directory";
            if (!string.IsNullOrEmpty(PatternCommondDownloadFolderPathText))
                folderBrowserDialog.SelectedPath = PatternCommondDownloadFolderPathText;
            folderBrowserDialog.ShowDialog();
            PatternCommondDownloadFolderPathText = folderBrowserDialog.SelectedPath;
        }

        private void ClosePatternEntry(object sender, EventArgs args)
        {
            PatternEntries.Remove(sender as PatternEntryViewModel);
        }

        private void OnPatternEtriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (PatternEntries.Count == 1)
                PatternEntries[0].PatternCustomDownloadFolderInputVisibility = Visibility.Collapsed;
            else
                if(PatternEntries.Count > 1)
                    PatternEntries[0].PatternCustomDownloadFolderInputVisibility = Visibility.Visible;
        }
    }
}
