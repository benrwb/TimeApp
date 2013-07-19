using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
            this.Hide();

            Timer t = new Timer();
            t.Interval = 60000;
            t.Elapsed += t_Elapsed;
            t.Start();

            Timer t2 = new Timer();
            t2.Interval = 1000;
            t2.Elapsed += t_Elapsed;


            t2.AutoReset = false;
            t2.Start();

        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
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




            Dispatcher.Invoke((Action)delegate
            {
                label1.Content = DateTime.Now.ToString("h:mm");
                this.Show();
            });

            Brush color = (level == 2 ? Brushes.Red : Brushes.LimeGreen);
            for (int i = 0; i < 5; i++)
            {
                Dispatcher.Invoke((Action)delegate
                {
                    label1.Foreground = (i % 2 == 0) ? color : Brushes.Black;
                    this.Background = (i % 2 != 0) ? color : Brushes.Black;
                });
                System.Threading.Thread.Sleep(250);
            }


            Dispatcher.Invoke((Action)delegate
            {
                this.Hide();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ewh.Close();
        }
    }
}
