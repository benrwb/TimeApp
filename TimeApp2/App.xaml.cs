using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace TimeApp2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void Log(string msg)
        {
            using (var sr = new StreamWriter(@"c:\users\user\log.txt", true))
            {
                sr.WriteLine(DateTime.Now + "\t" + Assembly.GetEntryAssembly().Location + "\t" + msg);
            }
        }




        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Log("App started");
        }

       

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Log("App exit");
        }
    }
}
