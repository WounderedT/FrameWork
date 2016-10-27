using FrameWork.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

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
        }

        void TimeTrackerDispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string excpetionText = ex.Message + Environment.NewLine + ex.StackTrace;
            string windowText = "Ups! This is embarrassing... :(" + Environment.NewLine;
            MessageBox.Show(windowText + excpetionText, ex.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //[DllImport("dwmapi.dll", PreserveSig = false)]
        //static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);


        //[DllImport("dwmapi.dll", PreserveSig = false)]
        //static extern bool DwmIsCompositionEnabled();

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            // This can’t be done any earlier than the SourceInitialized event:
            //var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            //hwndSource.AddHook(WndProc);

            //GlassHelper.RemoveNonClientRegion(this);
            //GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
            //GlassHelper.SetWindowThemeAttribute(this, false, false);
        }

        //private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    if (msg == 0x0083)
        //        return new IntPtr(0);
        //    return new IntPtr(0);
        //}
    }

    public class GlassHelper
    {
        public static bool ExtendGlassFrame(Window window, Thickness margin)
        {
            if (!NativeMethods.DwmIsCompositionEnabled())
                return false;


            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException("The Window must be shown before extending glass.");


            // Set the background to transparent from both the WPF and Win32 perspectives
            window.Background = Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;


            MARGINS margins = new MARGINS(margin);
            NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);
            return true;
        }

        public static void SetWindowThemeAttribute(Window window, bool showCaption, bool showIcon)
        {
            bool isGlassEnabled = NativeMethods.DwmIsCompositionEnabled();

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException("The Window must be shown before extending glass.");

            var options = new NativeMethods.WTA_OPTIONS
            {
                dwMask = (NativeMethods.WTNCA.NODRAWCAPTION | NativeMethods.WTNCA.NODRAWICON)
            };
            if (isGlassEnabled)
            {
                if (!showCaption)
                {
                    options.dwFlags |= NativeMethods.WTNCA.NODRAWCAPTION;
                }
                if (!showIcon)
                {
                    options.dwFlags |= NativeMethods.WTNCA.NODRAWICON;
                }
            }

            NativeMethods.SetWindowThemeAttribute(hwnd, NativeMethods.WINDOWTHEMEATTRIBUTETYPE.WTA_NONCLIENT, ref options, NativeMethods.WTA_OPTIONS.Size);
        }

        public static void RemoveNonClientRegion(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException("The Window must be shown before extending glass.");

            NativeMethods.SetWindowPos(hwnd, hwnd, 0, 0, 0, 0, NativeMethods.SWP.NOSIZE | NativeMethods.SWP.FRAMECHANGED);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("uxtheme.dll")]
        public static extern void SetWindowThemeAttribute(IntPtr hwnd, WINDOWTHEMEATTRIBUTETYPE eAttribute, ref WTA_OPTIONS options, uint cbAttribute);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags);

        public enum WTNCA : uint
        {
            NODRAWCAPTION = 0x00000001,
            NODRAWICON = 0x00000002,
            NOSYSMENU = 0x00000004,
            NOMIRRORHELP = 0x00000008,
            VALIDBITS = NODRAWCAPTION | NODRAWICON | NOMIRRORHELP | NOSYSMENU,
        }

        public struct WTA_OPTIONS
        {
            public const uint Size = 8;
            public WTNCA dwFlags;
            public WTNCA dwMask;
        }

        public enum WINDOWTHEMEATTRIBUTETYPE
        {
            WTA_NONCLIENT = 1
        }

        internal enum SWP
        {
            ASYNCWINDOWPOS = 0x4000,
            DEFERERASE = 0x2000,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            HIDEWINDOW = 0x0080,
            NOACTIVATE = 0x0010,
            NOCOPYBITS = 0x0100,
            NOMOVE = 0x0002,
            NOOWNERZORDER = 0x0200,
            NOREDRAW = 0x0008,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            NOSIZE = 0x0001,
            NOZORDER = 0x0004,
            SHOWWINDOW = 0x0040,
        }
    }

    public struct MARGINS
    {
        public MARGINS(Thickness t)
        {
            Left = (int)t.Left;
            Right = (int)t.Right;
            Top = (int)t.Top;
            Bottom = (int)t.Bottom;
        }
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }
}
