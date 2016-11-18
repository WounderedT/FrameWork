using System;
using System.Reflection;
using System.Windows;

namespace FrameWork
{
    public static class StaticResources
    {
        private static string _resourceDictionaryName = @"pack://application:,,,/Resources/ResourceDictionaries/StaticWindowParameters.xaml";

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
        
        /* 
         * Minimum width of header system area. Area consists of new tab button, window drag area, window minimize and close buttons.
         */
        public static double MinSystemAreaWidth { get; set; }
        
        /* 
         * Combined width of header system area buttons. Buttons are: new tab button, window minimize and close buttons.
         */
        public static double SystemButtonAreaWidth { get; set; }
        
        /* 
         * Total closable tab headers area. 
         */
        public static double TabAreaWidth { get; set; }
       
        /* 
         * Total width of window's dynamic header part.
         */
        public static double DynamicWindowAreaWidth { get; set; }

        /* 
         * Closable tab header paddings.
         */
        public static Thickness TabHeaderPadding { get; set; }

        public static void InitializeResources()
        {
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri(_resourceDictionaryName);
            foreach (PropertyInfo property in typeof(StaticResources).GetProperties())
            {
                try
                {
                    if(rd.Contains(property.Name))
                        property.SetValue(property, rd[property.Name]);
                }
                catch (ArgumentException) { }
            }

            MinSystemAreaWidth = NewTabButtonSize + SystemButtonWidth * 2 + WindowDragAreaMinWidth;
            SystemButtonAreaWidth = NewTabButtonSize + SystemButtonWidth * 2;
            TabAreaWidth = MainWindowWidth - MinSystemAreaWidth;
            DynamicWindowAreaWidth = MainWindowWidth - SystemButtonAreaWidth;
            TabHeaderPadding = new Thickness(2);
        }
    }
}
