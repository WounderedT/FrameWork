using FrameWork.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWork
{
    public static class StaticResources
    {
        public static double MainWindowHeight { get; set; }
        public static double MainWindowWidth { get; set; }

        public static double NewTabButtonSize { get; set; }
        public static double WindowDragAreaMinWidth { get; set; }
        public static double SystemButtonHeight { get; set; }
        public static double SystemButtonWidth { get; set; }

        public static double TabCloseButtonWidth { get; set; }
        public static double TabTitleDefaultWidth { get; set; }

        public static double MinSystemAreaWidth { get; set; }
        public static double SystemButtonAreaWidth { get; set; }
        public static double TabAreaWidth { get; set; }
        public static Thickness TabHeaderPadding { get; set; }
        public static double TabHeaderTotalPadding { get; set; }
        public static double TabHeaderDefaultWidth { get; set; }

        private static bool _isInitialized = false;
        private static string _resourceDictionaryName = "MainWindowDictionary.xaml";

        public static void InitializeResources()
        {
            if (_isInitialized)
                return;
            bool cleanUp = false;
            var mergedDict = Application.Current.Resources.MergedDictionaries.Where(w => w.Source.Equals(_resourceDictionaryName)).FirstOrDefault();
            if(mergedDict == null)
            {
                ResourceDictionary rd = new ResourceDictionary();
                rd.Source = new Uri(_resourceDictionaryName, UriKind.Relative);
                Application.Current.Resources.MergedDictionaries.Add(rd);
                mergedDict = Application.Current.Resources.MergedDictionaries.Where(w => w.Source.Equals(_resourceDictionaryName)).FirstOrDefault();
                cleanUp = true;
            }
            foreach (PropertyInfo property in typeof(StaticResources).GetProperties())
            {
                try
                {
                    if(mergedDict.Contains(property.Name))
                        property.SetValue(property, mergedDict[property.Name]);
                }
                catch (ArgumentException) { }
            }
            if (cleanUp)
                Application.Current.Resources.MergedDictionaries.Remove(mergedDict);

            MinSystemAreaWidth = NewTabButtonSize + SystemButtonWidth * 2 + WindowDragAreaMinWidth;
            SystemButtonAreaWidth = NewTabButtonSize + SystemButtonWidth * 2;
            TabAreaWidth = MainWindowWidth - MinSystemAreaWidth;
            TabHeaderPadding = new Thickness(2);
            TabHeaderTotalPadding = TabHeaderPadding.Left + TabHeaderPadding.Right;
            TabHeaderDefaultWidth = TabTitleDefaultWidth + TabCloseButtonWidth + TabHeaderTotalPadding;
        }
    }
}
