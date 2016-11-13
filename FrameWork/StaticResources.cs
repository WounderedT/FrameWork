using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace FrameWork
{
    public static class StaticResources
    {
        private static string _resourceDictionaryName = "Resources/ResourceDictionaries/StaticWindowParameters.xaml";
        
        //Static parameters from resource file
        public static double MainWindowHeight { get; set; }
        public static double MainWindowWidth { get; set; }
        public static double NewTabButtonSize { get; set; }
        public static double WindowDragAreaMinWidth { get; set; }
        public static double SystemButtonHeight { get; set; }
        public static double SystemButtonWidth { get; set; }
        public static double TabCloseButtonDefaultWidth { get; set; }
        public static double TabTitleDefaultWidth { get; set; }

        //parameters based on Static parameters
        public static double MinSystemAreaWidth { get; set; }
        public static double SystemButtonAreaWidth { get; set; }
        public static double TabAreaWidth { get; set; }
        public static double DynamicWindowAreaWidth { get; set; }
        public static double TabCloseButtonWidth { get; set; }
        public static Thickness TabHeaderPadding { get; set; }

        public static void InitializeResources()
        {
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

            TabCloseButtonWidth = TabCloseButtonDefaultWidth;
            MinSystemAreaWidth = NewTabButtonSize + SystemButtonWidth * 2 + WindowDragAreaMinWidth;
            SystemButtonAreaWidth = NewTabButtonSize + SystemButtonWidth * 2;
            TabAreaWidth = MainWindowWidth - MinSystemAreaWidth;
            DynamicWindowAreaWidth = MainWindowWidth - SystemButtonAreaWidth;
            TabHeaderPadding = new Thickness(2);
        }
    }
}
