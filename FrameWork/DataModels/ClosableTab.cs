using FrameWork.ViewModel;
using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FrameWork.DataModels
{
    public class ClosableTab: TabItem
    {
        TabCloseViewModel closeView = null;

        public event PluginTabCloseEventHandler PluginTabClose;
        public delegate void PluginTabCloseEventHandler(object caller, PluginTabCloseEventArgs args);

        private double _defaultTitleWidth;

        public bool CleanProgressBar
        {
            get; set;
        }

        public string Title
        {
            get
            {
                return (string) closeView.LabelContent;
            }
            set
            {
                closeView.LabelContent = value;
            }
        }

        public Brush HeaderBackground
        {
            get
            {
                return closeView.LabelBackground;
            }
            set
            {
                closeView.LabelBackground = value;
            }
        }

        public double HeaderWidth
        {
            get
            {
                return closeView.ClosableTabLabelWidth;
            }
            set
            {
                closeView.ClosableTabLabelWidth = value;
                //if (value != NewHeaderWidth)
                //    NewHeaderWidth = value;
            }
        }

        //public double NewHeaderWidth
        //{
        //    get { return _newTitleWidth; }
        //    set { _newTitleWidth = value; }
        //}

        public double CloseTabButtonWidth
        {
            get { return closeView.TabCloseButtonWidth; }
            set { closeView.TabCloseButtonWidth = value; }
        }

        public bool CanClose { get; set; }

        public ClosableTab()
        {
            ClosableTabConstructor(true);
        }

        public ClosableTab(bool canClose)
        {
            ClosableTabConstructor(canClose);
        }

        private void ClosableTabConstructor(bool canClose)
        {
            closeView = new TabCloseViewModel();
            closeView.LabelContent = "Unnamed";
            closeView.CloseButtonClick = new RelayCommand(param => buttonTabClose_Click(Title), param => buttonTabClose_CanExecute());
            CanClose = canClose;
            if(!CanClose)
                closeView.ButtonCloseVisibility = Visibility.Collapsed;
            Header = closeView;
            CleanProgressBar = false;
            Padding = StaticResources.TabHeaderPadding;
        }


        public void CreateProgressBar(object sender, EventArgs args)
        {
            SolidColorBrush solid = new SolidColorBrush();
            solid.Color = GetColor();
            HeaderBackground = solid;
        }

        public void UpdateProgressBar(object sender, EventArgs args)
        {
            var progressArgs = (IProgressReportUpdateEventArgs) args;
            if(progressArgs.Value < 1.0)
            {
                LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                myLinearGradientBrush.StartPoint = new Point(0, 0.5);
                myLinearGradientBrush.EndPoint = new Point(1, 0.5);
                Color color = GetColor(progressArgs.Status);
                myLinearGradientBrush.GradientStops.Add(new GradientStop(color, 0.0));
                myLinearGradientBrush.GradientStops.Add(new GradientStop(GetColor(), progressArgs.Value));
                HeaderBackground = myLinearGradientBrush;
            }
            else
            {
                SolidColorBrush solid = new SolidColorBrush();
                solid.Color = GetColor(progressArgs.Status);
                HeaderBackground = solid;
            }
            
        }

        public void ProgressComplete(object sender, EventArgs args)
        {
            CleanProgressBar = true;
        }

        public void ClearProgressBarEventHandler(object sender, EventArgs args)
        {
            if (IsSelected)
            {
                ClearProgressBar();
            }
            else
            {
                CleanProgressBar = true;
            }
        }

        public void ClearProgressBar()
        {
            SolidColorBrush empty = new SolidColorBrush();
            empty.Color = GetColor();
            HeaderBackground = empty;
        }

        public void shrinktitle(double value)
        {
            _defaultTitleWidth = HeaderWidth;
            HeaderWidth = value;
        }

        public void restoretitle()
        {
            HeaderWidth = _defaultTitleWidth;
        }

        public double GetPaddingWidth()
        {
            return Padding.Left + Padding.Right;
        }

        private Color GetColor(string status = "")
        {
            switch (status)
            {
                case "OK":
                    return Colors.LimeGreen;
                case "Error":
                    return Colors.Red;
                default:
                    return Colors.Transparent;
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            if (CanClose)
            {
                closeView.ButtonCloseVisibility = Visibility.Visible;
            }
            if (CleanProgressBar)
            {
                ClearProgressBar();
                CleanProgressBar = false;
            }
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            if (CanClose)
            {
                closeView.ButtonCloseVisibility = Visibility.Hidden;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (CanClose)
            {
                closeView.ButtonCloseVisibility = Visibility.Visible;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsSelected && CanClose)
            {
                closeView.ButtonCloseVisibility = Visibility.Hidden;
            }
        }

        private void buttonTabClose_Click(string title)
        {
            OnPluginTabClose(new PluginTabCloseEventArgs(title));
        }

        private bool buttonTabClose_CanExecute()
        {
            return CanClose;
        }

        protected virtual void OnPluginTabClose(PluginTabCloseEventArgs args)
        {
            PluginTabClose?.Invoke(this, args);
        }

        //protected override void OnRender(DrawingContext dc)
        //{
        //    //base.OnRender(dc);
        //    //if (NewHeaderWidth > 0 && NewHeaderWidth != HeaderWidth)
        //    //    HeaderWidth = NewHeaderWidth;
        //    /*This check is required to update UI in case of dynamic header size which could be loaded from color scheme.
        //     * If different color scheme is loaded and header size is changed UI must be updated to reflect these changes.
        //     * All these updates must be as lightweight as possibe and only to be invoken if header with greater width is loaded.
        //     */
        //    if (ActualWidth > StaticResources.TabHeaderDefaultWidth + Math.Abs(Margin.Left) + Math.Abs(Margin.Right) || Settings.IsColorSchemeUpdated)
        //    //if (ActualWidth != StaticResources.TabHeaderDefaultWidth)
        //    {
        //        //if (ActualWidth == StaticResources.TabHeaderDefaultWidth + Math.Abs(Margin.Left) + Math.Abs(Margin.Right))
        //        //    return;
        //        //StaticResources.TabCloseButtonWidth = ActualWidth - StaticResources.TabTitleDefaultWidth
        //        //    - StaticResources.TabHeaderTotalPadding + Margin.Left + Margin.Right;

        //        //StaticResources.TabCloseButtonWidth = 50;
        //        //StaticResources.RecalculateParameters();

        //        //Session.OnUpdateDragAreaWidth(new UpdateDragAreaWidthEventArgs(
        //        //    StaticResources.MainWindowWidth - StaticResources.TabHeaderDefaultWidth * Session.Tabs.Count - StaticResources.SystemButtonAreaWidth));

        //        //closeView = new TabCloseViewModel() { LabelContent = "TEST" };
        //        ////closeView.TabCloseButtonWidth = StaticResources.TabCloseButtonWidth;
        //        //Header = closeView;
        //        //Session.UpdateTabsHeaderWidth();

        //        Session.UpdateUIWidth(null, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        //    }
        //}
    }

    public class PluginTabCloseEventArgs : EventArgs
    {
        public string pluginName;

        public PluginTabCloseEventArgs(string pluginName)
        {
            this.pluginName = pluginName;
        }
    }
}
