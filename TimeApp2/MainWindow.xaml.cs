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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeApp2
{
    public partial class MainWindow : Window
    {
        System.Threading.EventWaitHandle ewh; // support for running on multiple desktops


        // TODO: Hook.UnhookWindowsHook();    on Application.Exit


        public MainWindow()
        {
            ewh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset, @"Global\TIMEAPP-" + Desktops.GetProcessDesktopName());

            InitializeComponent();




            Timer t = new Timer() { Interval = 1000 };
            t.Elapsed += CheckTime;
            t.Start();

            var t3 = new Timer() { Interval = 100, AutoReset = false };
            t3.Elapsed += BeginFollowMouse;
            t3.Start();
        }

        void BeginFollowMouse(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate
           {

               // FOLLOW MOUSE CURSOR
               //      (HWND is not available until AFTER .Show() has been called)
               var hwnd = new WindowInteropHelper(this).Handle;

               //var starting_pos = GetMousePosition();
               //SetWindowPos(hwnd, IntPtr.Zero, starting_pos.X + 15, starting_pos.Y + 15, 0, 0, SWP_NOZORDER | SWP_NOSIZE);

               Action<Hook.MSLLHOOKSTRUCT> callback = p =>
               {
                   SetWindowPos(hwnd, HWND_TOPMOST, p.pt.x + 15, p.pt.y + 15, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE /*| SWP_NOZORDER*/);
               };
               Hook.SetWindowsHook(callback);
           });

        }

        void CheckTime(object sender, ElapsedEventArgs e)
        {
            // BEGIN multiple desktops support
            // Check for synchronisation object for currently active desktop.
            // If it can't be found, start a new process.
            string currentDesktopName = Desktops.GetCurrentDesktopName();
            if (currentDesktopName.StartsWith("Sysinternals Desktop")) // user is on another desktop (will already be running on "Default" desktop)
            {
                try
                {
                    var h = System.Threading.EventWaitHandle.OpenExisting(@"Global\TIMEAPP-" + currentDesktopName);
                    h.Close();
                    // found (no exception thrown)
                }
                catch (System.Threading.WaitHandleCannotBeOpenedException)
                {
                    // not found
                    var thisasm = System.Reflection.Assembly.GetExecutingAssembly();
                    Desktops.StartProcessOnDesktop(thisasm.Location, currentDesktopName);
                }
            }
            // END multiple desktops support





            DateTime now = DateTime.Now;

            var target_time = new TimeSpan(22, 00, 00);
            TimeSpan diff = now.TimeOfDay - target_time;
            var prefix = diff.Ticks < 0 ? "-" : "+";

            string labelText = prefix + diff.ToString("hh\\:mm");
            
            Dispatcher.Invoke((Action)delegate
            {
                label1.Content = labelText;
                label1.Foreground = new SolidColorBrush(diff.Ticks < 0 ? Colors.Black : Colors.Red);

                // Hide the label not the window (i.e. with this.Hide()) 
                // because then SetWindowPos would be trying to position a window which "doesn't exist"
                // which for some reason disables left-click system-wide(!)
                label1.Visibility = (diff.TotalMinutes > -60)
                    ? Visibility.Visible
                    : Visibility.Hidden;
            });

            if (prevLabelText != null && !labelText.Equals(prevLabelText))
            {
                new System.Threading.Thread(Blink).Start();// blink every minute to attract attention
            }
            prevLabelText = labelText;

        }

        string prevLabelText = null;


        void Blink()
        {
            for(int i = 0; i < 2; i++)
            {
                Dispatcher.Invoke((Action)delegate
                {
                    label1.Background = new SolidColorBrush(Colors.White);
                });
                System.Threading.Thread.Sleep(250);

                Dispatcher.Invoke((Action)delegate
                {
                    label1.Background = null;
                });
                System.Threading.Thread.Sleep(250);
            }
        }






        

        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        const int SWP_NOZORDER = 0x0004;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOACTIVATE = 0x0010;
        IntPtr HWND_TOPMOST = new IntPtr(-1);


    }
}
