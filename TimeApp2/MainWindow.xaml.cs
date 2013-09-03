using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using System.Timers;
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

namespace TimeApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Threading.EventWaitHandle ewh; // support for running on multiple desktops


        public MainWindow()
        {
            ewh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset, @"Global\TIMEAPP-" + Desktops.GetProcessDesktopName());

            InitializeComponent();




            Timer t = new Timer() { Interval = 60000 };
            t.Elapsed += t_Elapsed;
            t.Start();

            Timer t2 = new Timer() { Interval = 1000, AutoReset = false };
            t2.Elapsed += t_Elapsed;
            t2.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            if (now.Hour < 5) now = now.AddDays(-1);
            // e.g. late Saturday night is technically early Sunday morning, 
            //      but we want to class it as Saturday instead.

            int level = 0;
            if (new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday }.Contains(DateTime.Now.DayOfWeek))
            {
                if (DateTime.Now.Hour == 21 && DateTime.Now.Minute % 5 == 0)
                {
                    level = 1;
                }
                else if (new[] { 22, 23, 0 }.Contains(DateTime.Now.Hour))
                {
                    level = 2;
                }
            }
            if (level == 0) return; // nothing to do



            // BEGIN multiple desktops support
            // Check for synchronisation object for currently active desktop.
            // If it can't be found, start a new process.
            string currentDesktopName = Desktops.GetCurrentDesktopName();
            try
            {
                var h = System.Threading.EventWaitHandle.OpenExisting(@"Global\TIMEAPP-" + currentDesktopName);
                h.Close();
                // found
            }
            catch (System.Threading.WaitHandleCannotBeOpenedException)
            {
                // not found
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                Desktops.StartProcessOnDesktop(asm.Location, currentDesktopName);
            }
            // END multiple desktops support



            new System.Threading.Thread(pos).Start(); // follow the mouse cursor for 1.2 seconds
            System.Threading.Thread.Sleep(100);// allow time for thread to start

            Dispatcher.Invoke((Action)delegate
            {
                label1.Content = DateTime.Now.ToString("h:mm");
                this.Show();
            });

            Brush color = (level == 2 ? Brushes.Red : Brushes.LimeGreen);
            Brush altcolor = (level == 2 ? Brushes.Black : Brushes.White);
            for (int i = 0; i < 5; i++)
            {
                Dispatcher.Invoke((Action)delegate
                {
                    label1.Foreground = (i % 2 == 0) ? color : altcolor;
                    border1.Background = (i % 2 != 0) ? color : altcolor;
                });
                System.Threading.Thread.Sleep(250);
            }


            Dispatcher.Invoke((Action)delegate
            {
                this.Hide();
            });
        }




        void pos()
        {

            IntPtr hwnd = IntPtr.Zero;
            while (hwnd == IntPtr.Zero)
            {
                Dispatcher.Invoke((Action)delegate
                {
                    hwnd = new WindowInteropHelper(this).Handle;
                });
                if (hwnd == IntPtr.Zero) { System.Threading.Thread.Sleep(100); } // wait for window handle to become available
            }

            int ms = 15;  // 15ms ~ 60fps 
            // ms to fps
            // 1000 = 1
            // 500  = 2
            // 250  = 4
            // 125  = 8
            // 62.5 = 16
            // 31.25 = 32
            // 15.625 = 64

            for (int i = 0; i < 1200; i += ms) // follow the mouse cursor for 1.2 seconds, then exit
            {
                var pos = GetMousePosition();

                SetWindowPos(hwnd, IntPtr.Zero, (int)pos.X + 10, (int)pos.Y + 10, 0, 0, SWP_NOZORDER | SWP_NOSIZE);

                System.Threading.Thread.Sleep(ms);

            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        const int SWP_NOZORDER = 0x0004;
        const int SWP_NOSIZE = 0x0001;



    }
}
