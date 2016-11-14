using FrameWork.ViewModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace FrameWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel = new MainWindowViewModel();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.LoadAuthentificationTab();
        }

        void TimeTrackerDispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string excpetionText = ex.Message + Environment.NewLine + ex.StackTrace;
            string windowText = "Ups! This is embarrassing... :(" + Environment.NewLine;
            MessageBox.Show(windowText + excpetionText, ex.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (!viewModel.mutex.WaitOne(0, false))
            {
                MessageBox.Show("Instance already running", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            /*Remove window menu from main window's title bar*/
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE, new IntPtr(NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_STYLE) & ~NativeMethods.WS_SYSMENU));
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
                /*Add wndProc hook to handle window resize messages.*/
                hwndSource.AddHook(WndProc);
        }

        private void OnDragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_NCHITTEST)
            {
                handled = true;
                var htLocation = NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam).ToInt32();
                switch (htLocation)
                {
                    case NativeMethods.HTBOTTOM:
                    case NativeMethods.HTBOTTOMLEFT:
                    case NativeMethods.HTBOTTOMRIGHT:
                    case NativeMethods.HTLEFT:
                    case NativeMethods.HTRIGHT:
                    case NativeMethods.HTTOP:
                    case NativeMethods.HTTOPLEFT:
                    case NativeMethods.HTTOPRIGHT:
                        htLocation = NativeMethods.HTBORDER;
                        break;
                }

                return new IntPtr(htLocation);
            }

            return IntPtr.Zero;
        }
    }

    internal static class NativeMethods
    {
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;
        public const int WM_NCHITTEST = 0x0084;
        public const int HTBORDER = 18;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;

        /*Calls the default window procedure to provide default processing for any window messages
         *that an application does not process. This function ensures that every message is processed.
         */
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /*Retrieves information about the specified window. The function also retrieves the 32-bit (DWORD) 
         *value at the specified offset into the extra window memory. 
         */
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /*Changes an attribute of the specified window. The function also sets a value at the specified 
         *offset in the extra window memory. 
         */
        // This is aliased as a macro in 32bit Windows.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr dwNewLong)
        {
            if (8 == IntPtr.Size)
            {
                return SetWindowLongPtr64(hwnd, nIndex, dwNewLong);
            }
            return new IntPtr(SetWindowLongPtr32(hwnd, nIndex, dwNewLong.ToInt32()));
        }

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
