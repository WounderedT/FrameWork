using Downloader.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Downloader
{
    [Serializable]
    public class Pattern
    {
        private const int HalfKeyLength = 4;
        public const String Default = "Default";

        public static string KeySeparator { get; } = "#@#_{0}_#@#";
        
        public static Regex KeyRegEx { get; } = new Regex("#@#_[A-Za-z0-9_]*_#@#");

        public static string Indexer { get; } = string.Format(KeySeparator, "INDEXER");

        public static Regex IndexerRegEx { get; } = new Regex(Indexer);

        public static int IndexerLength
        {
            get { return Indexer.Length; }
        }

        public static char CollapsedIndexer
        {
            get { return char.MaxValue; }
        }

        public string Name { get; set; }
        public string LinkPattern { get; set; }
        public List<PatternKeyViewModel> Keys { get; set; }
        public string GlobalFileName { get; set; }
        public string Extension { get; set; }
        public bool SkipMissingFiles { get; set; }
        public bool IsChanged { get; set; }
        public bool EditMode { get; set; }

        public Visibility GlobalFileNameVisibility { get; set; }

        public bool CanChange { get; }

        public bool CanSave
        {
            get
            {
                if (Name.Equals(Default))
                    return true;
                if (EditMode && IsChanged)
                    return true;
                else
                    return false;
            }
        }

        public bool CanEdit
        {
            get
            {
                if (Name.Equals(Default))
                    return false;
                else
                    return true;
            }
        }

        public bool CanRemove
        {
            get
            {
                if (Name.Equals(Default))
                    return false;
                else
                    return true;
            }
        }

        public Pattern(String name = Default, bool changable = true)
        {
            CanChange = changable;
            Keys = new List<PatternKeyViewModel>();
            Name = name;
        }

        public static string UpdateLink()
        {
            throw new NotImplementedException("Pattern update method is not implemented yet. Use default pattern instead.");
        }

        public IEnumerable<Tuple<int, string>> GetLinkPatternPositions()
        {
            if (string.IsNullOrEmpty(LinkPattern))
                return null;
            return LinkPattern.Split(';').Where(w => !string.IsNullOrEmpty(w)).Select(
                w => new Tuple<int, string>(int.Parse(w.Split(',').First()),
                Keys[Keys.IndexOf(Keys.Where(i => i.KeyName.Equals(w.Split(',').Last())).FirstOrDefault())].KeyValueIsInterval ? "&"
                + Keys[Keys.IndexOf(Keys.Where(i => i.KeyName.Equals(w.Split(',').Last())).FirstOrDefault())].KeyName + "_IntervalIndex&" :
                Keys[Keys.IndexOf(Keys.Where(i => i.KeyName.Equals(w.Split(',').Last())).FirstOrDefault())].KeyValue));
        }

        public void Parse(PatternEntryViewModel sender)
        {
            if(sender.PatternKeysList.Count > 0)
                if(sender.PatternKeysList.Where(w => w.PatternEntryKeyVisibility.Equals(Visibility.Visible)).Count() > 0)
                {
                    var split = sender.PatternDownloadLinkText.Split('/');
                    for(int ind = 0;ind< split.Count(); ind++)
                    {
                        if (split[ind].Contains(KeySeparator))
                        {
                            var keySplit = split[ind].Split(new string[] { KeySeparator }, StringSplitOptions.None);
                            for (int keyInd = 1; keyInd < keySplit.Length; keyInd += 2)
                            {
                                LinkPattern += ind + "," + keySplit[keyInd] + ";";
                                PatternKeyViewModel key = sender.PatternKeysList.Where(w => w.KeyName.Equals(keySplit[keyInd])).FirstOrDefault();
                                Keys.Add(key);
                            }
                        }
                    }
                }

            Extension = "." + sender.PatternDownloadLinkText.Split('/').Last().Split('.').Last();
            if (string.IsNullOrEmpty(sender.PatternGlobalFileNameText))
                GlobalFileNameVisibility = Visibility.Collapsed;
            else
                GlobalFileNameVisibility = Visibility.Visible;

            SkipMissingFiles = sender.SkipMissingFilesCheck;
        }

        public static string GetKeyNameFromKeyString(string keyNameString)
        {
            return keyNameString.Substring(HalfKeyLength, keyNameString.Length - HalfKeyLength * 2);
        }
    }
}
