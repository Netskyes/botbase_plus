using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Aeon.Internal.UI;

namespace Aeon.Internal
{
    public static class AppInternal
    {
        internal static MainWindow MainWindow { get; set; }
        internal static PluginsHelper PluginsHelper { get; set; }
        internal static NetworkManager NetworkManager { get; set; }
        internal static CoreManager CoreManager { get; set; }
        internal static SQLiteClient SQLiteClient { get; set; }

        static AppInternal()
        {
        }

        public static void Init()
        {
            CoreManager = new CoreManager();
            PluginsHelper = new PluginsHelper();
            NetworkManager = new NetworkManager();
            SQLiteClient = new SQLiteClient("base.db3");

            RunMainWindow();
        }

        public static void Log(string text, string tabName = "Main")
        {
            MainWindow.Log(text, tabName);
        }

        private static void RunMainWindow()
        {
            try
            {
                var thread = new Thread(() =>
                {
                    MainWindow = new MainWindow();
                    MainWindow.Show();
                    MainWindow.Closed += MainWindow_Closed;

                    Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Priority = ThreadPriority.AboveNormal;
                thread.Start();
            }
            catch (Exception e)
            {
                Utils.Log(e.Message + " " + e.StackTrace, "error");
            }
        }

        private static void MainWindow_Closed(object sender, EventArgs e)
        {
        }
    }
}
