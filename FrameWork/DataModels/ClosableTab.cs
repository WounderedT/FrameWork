using FrameWork.ViewModel;
using Interface;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FrameWork.DataModels
{
    public class ClosableTab: TabItem
    {
        private TabCloseViewModel _closeView = null;

        public event PluginTabCloseEventHandler PluginTabClose;
        public delegate void PluginTabCloseEventHandler(object caller, PluginTabCloseEventArgs args);

        public bool CleanProgressBar
        {
            get; set;
        }

        public string Title
        {
            get
            {
                return (string) _closeView.LabelContent;
            }
            set
            {
                _closeView.LabelContent = value;
            }
        }

        public Brush HeaderBackground
        {
            get
            {
                return _closeView.LabelBackground;
            }
            set
            {
                _closeView.LabelBackground = value;
            }
        }

        public double HeaderWidth
        {
            get
            {
                return _closeView.ClosableTabLabelWidth;
            }
            set
            {
                _closeView.ClosableTabLabelWidth = value;
            }
        }

        public double CloseTabButtonWidth
        {
            get { return _closeView.TabCloseButtonWidth; }
            set { _closeView.TabCloseButtonWidth = value; }
        }

        public bool CanClose { get; set; }

        public ClosableTab() : this(true) { }

        public ClosableTab(bool canClose)
        {
            _closeView = new TabCloseViewModel();
            _closeView.LabelContent = "Unnamed";
            CanClose = canClose;
            _closeView.CloseButtonClick = new RelayCommand(param => buttonTabClose_Click(Title), param => buttonTabClose_CanExecute());
            if (!CanClose)
                _closeView.ButtonCloseVisibility = Visibility.Collapsed;
            else
                _closeView.ButtonCloseVisibility = Visibility.Hidden;
            Header = _closeView;
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

        public void ClearProgressBarEventHandler(object sender, EventArgs args)
        {
            if (IsSelected)
                ClearProgressBar();
            else
                CleanProgressBar = true;
        }

        public void ClearProgressBar()
        {
            SolidColorBrush empty = new SolidColorBrush();
            empty.Color = GetColor();
            HeaderBackground = empty;
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
                _closeView.ButtonCloseVisibility = Visibility.Visible;
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
                _closeView.ButtonCloseVisibility = Visibility.Hidden;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (CanClose)
                _closeView.ButtonCloseVisibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsSelected && CanClose)
                _closeView.ButtonCloseVisibility = Visibility.Hidden;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            var tab = e.Source as ClosableTab;
            if (tab == null)
                return;

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
                DragDrop.DoDragDrop(tab, tab, DragDropEffects.All);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            var tabTarget = e.Source as ClosableTab;
            var tabSource = e.Data.GetData(typeof(ClosableTab)) as ClosableTab;

            if (!tabSource.Equals(tabTarget))
            {
                /* Tab management is ought to be handled by Parent property of e.Source. Currently there is a bug(?) and this property
                 * is null for all objects in MainWindowViewModel.tabs(as well as MainWindowViewModel.Tabs). Untill this is changed tab rearrangement
                 * must be handler either by direct use of MainWindowViewModel.tabs or by implemenring OnDragEvent in Session.
                 */
                int targetIndex = MainWindowViewModel.tabs.IndexOf(tabTarget);
                int sourceIndex = MainWindowViewModel.tabs.IndexOf(tabSource);
                MainWindowViewModel.tabs.Move(sourceIndex, targetIndex);
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
