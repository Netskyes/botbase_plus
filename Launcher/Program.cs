using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
    class Program : Application
    {
        public void InitializeComponent()
        {
            base.Startup += Application_Startup;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "Aeon.Core.dll";
            Console.WriteLine("Launching: " + path);

            var core = Assembly.LoadFile(path);
            if (core != null)
            {
                var appMain = core.GetType("Aeon.AppMain");
                if (appMain != null)
                {
                    appMain.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke
                        (null, new object[] { new string[0] });
                }
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "debug")
            {
                Debug();
                return;
            }

            var program = new Program();
            program.InitializeComponent();
            program.Run();
        }

        public static void Debug()
        {
            using (var proc = new Process())
            {
                var info = new ProcessStartInfo(@"G:\Games\Glyph\Games\ArcheAge\Live\bin32\archeage.exe");
                info.Verb = "runas";
                info.Arguments = string.Format("-t +auth_ip {0} -auth_port {1} -handle {2} -lang {3}", 
                    "193.105.173.134", 1237,
                    "000008c8:000008c4",
                    "en_us"
                );
                proc.StartInfo = info;
                if (proc.Start())
                {
                    Console.WriteLine("Process started!");
                }
            }
        }
    }
}
