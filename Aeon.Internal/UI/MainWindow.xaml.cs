using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Aeon.Internal;
using System.Collections.ObjectModel;

namespace Aeon.Internal.UI
{
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<PluginModel> PluginListItems
        {
            get { return pluginListItems; }
        }

        private ObservableCollection<PluginModel> pluginListItems = new ObservableCollection<PluginModel>();

        internal LogDelegate Log;
        internal void Invoke(Action a) => Dispatcher.Invoke(a);
        internal PacketEditor PacketEditor { get; set; }

        public MainWindow()
        {
            Log = ConsoleLog;

            InitializeComponent();
            PluginsList.DataContext = this;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var test = new ProxyBase();
            //test.LaunchPlugin(@"G:\SoftwareDev\CSharp\BotRelay\Plugin\bin\Debug\Plugin.dll");

            UpdatePluginsList();
        }

        private void UpdatePluginsList()
        {
            foreach (var p in AppInternal.PluginsHelper.GetPlugins())
            {
                Invoke(() => pluginListItems.Add(new PluginModel { Path = p }));
            }
        }

        private void ConsoleLog(string text, string tabName)
        {
            Invoke(() => Console.AppendText(text + Environment.NewLine));
        }

        private void Btn_ShowPacketEditor_Click(object sender, RoutedEventArgs e)
        {
            if (PacketEditor != null 
                && PacketEditor.IsVisible)
                return;

            PacketEditor = new PacketEditor();
            PacketEditor.Show();
        }
    }
}
