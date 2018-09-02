using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NetRelay.Utils;
using NetRelay.Network;
using NetRelay.Network.Objects;
using NetRelay.Network.ClientExtensions;
using System.Net;
using System.IO;

namespace PacketEditor
{
    using Api;
    using Objects;
    using System.Reflection;

    public partial class MainWindow : Window
    {
        public ObservableCollection<Plugin> PluginItems
            { get => pluginItems; }

        public ObservableCollection<PacketModel> TCPPacketItems
            { get => tcpPacketItems; }

        public ObservableCollection<PacketModel> UDPPacketItems
            { get => udpPacketItems; }


        private Server server;
        private UdpServer udpServer;
        private const string processName = "ygopro_vs_links.exe";
        private ObservableCollection<PacketModel> tcpPacketItems = new ObservableCollection<PacketModel>();
        private ObservableCollection<PacketModel> udpPacketItems = new ObservableCollection<PacketModel>();
        private ObservableCollection<Plugin> pluginItems = new ObservableCollection<Plugin>();

        private PacketModel focusPacket;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PopulatePlugins();
            InitializeNetwork();
            
            BeginInjection("ygopro_vs_links.exe");
        }

        #region UI Network Handlers

        private void OnServerState(bool isListening)
        {
        }

        private void OnUdpClientState(Client client, bool isConnected)
        {
        }

        private void OnUdpClientSend(Client client, Packet packet)
        {
            if (packet is null)
                return;

            Invoke(() =>
            {
                if (udpPacketItems.Count > 600)
                    udpPacketItems.Clear();

                udpPacketItems.Add(new PacketModel
                {
                    ID = 2,
                    Client = null,
                    Remote = packet.EndPoint.ToString(),
                    Size = packet.Buffer.Length,
                    Content = Encoding.UTF8.GetString(packet.Buffer)
                });
            });
        }

        private void OnUdpClientRecv(Client client, Packet packet)
        {
            if (packet is null)
                return;

            Invoke(() =>
            {
                if (udpPacketItems.Count > 600)
                    udpPacketItems.Clear();

                udpPacketItems.Add(new PacketModel
                {
                    ID = 1,
                    Client = null,
                    Remote = packet.EndPoint.ToString(),
                    Size = packet.Buffer.Length,
                    Content = Encoding.UTF8.GetString(packet.Buffer),
                    Bytes = packet.Buffer
                });
            });
        }

        private void OnClientState(Server parent, Client client, bool isConnected)
        {
            if (isConnected)
            {
                ConsoleLog("Connected: " + client.EndPoint);
            }
        }

        private void OnClientRecv(Server parent, Client client, Packet packet)
        {
            var temp = client as ClientRelay;
            if (temp is null || packet is null)
                return;

            Invoke(() =>
            {
                tcpPacketItems.Insert(0, new PacketModel
                {
                    Client = client,
                    ID = temp.ID,
                    Type = "Recv",
                    Remote = temp.EndPoint.ToString(),
                    Size = packet.Buffer.Length,
                    Content = Encoding.UTF8.GetString(packet.Buffer),
                    Bytes = packet.Buffer
                });
            });
        }

        private void OnClientSend(Server parent, Client client, Packet packet)
        {
            var temp = client as ClientMain;
            if (temp is null || packet is null)
                return;

            Invoke(() =>
            {
                tcpPacketItems.Insert(0, new PacketModel
                {
                    Client = client,
                    ID = temp.ID,
                    Type = "Send",
                    Remote = temp.EndPoint.ToString(),
                    Size = packet.Buffer.Length,
                    Content = Encoding.UTF8.GetString(packet.Buffer),
                    Bytes = packet.Buffer
                });
            });
        }

        #endregion

        private void InitializeNetwork()
        {
            server = new Server();
            server.ServerState += OnServerState;
            // UI Event Handlers
            server.ClientSend += OnClientSend;
            server.ClientRecv += OnClientRecv;
            server.ClientState += OnClientState;

            udpServer = new UdpServer();
            // UI Event Handlers
            udpServer.ClientRecv += OnUdpClientRecv;
            udpServer.ClientSend += OnUdpClientSend;
            udpServer.ClientState += OnUdpClientState;

            server.Listen(8089);
            udpServer.BeginListenUdp(8089);

            if (server.Listening && udpServer.Listening)
            {
                ConsoleLog($"Server listening on: {server.Port}");
            }
        }

        private void PopulatePlugins()
            => Invoke(() => GetPlugins().ToList().ForEach(x => pluginItems.Add(x)));
        
        private void BeginInjection(string processName)
        {
            Task.Run(() =>
            {
                bool result;

                do
                {
                    result = Imports.Inject(processName, "G:/SoftwareDev/C#/BotRelay/Debug/Mapping.dll");
                    Thread.Sleep(200);
                }
                while (!result);

                ConsoleLog(processName + ": Injected to process!");
            });
        }

        public void ConsoleLog(string text)
        {
            Dispatcher.Invoke(() => txtbox_Console.AppendText(text + Environment.NewLine));
        }

        private void Invoke(Action a) => Dispatcher.Invoke(a);



        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (PacketModel)(sender as DataGridRow).DataContext;
            if (item == null)
                return;

            focusPacket = item;
            //var binary = item.Bytes.SelectMany(b => b.ToString("X"));
            //int i = 0;
            //var query = from b in binary let num = i++ group b by num / 2 into g select g.ToArray();

            var hex = BitConverter.ToString(item.Bytes).Replace("-", " ");

            Invoke(() =>
            {
                txtbox_Hex.Clear();
                //foreach (var b in query)
                //{
                //    txtbox_Hex.AppendText(string.Join("", b) + " ");
                //}

                txtbox_Hex.Text = hex;
            });
        }

        private void DataGridRow_PluginMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var plugin = (Plugin)(sender as DataGridRow).DataContext;
            if (plugin == null)
                return;

            ExecutePlugin(plugin.Path);
        }

        private void btn_ClearPackets_Click(object sender, RoutedEventArgs e)
        {
            tcpPacketItems.Clear();
            udpPacketItems.Clear();
        }

        private void btn_SendPacket_Click(object sender, RoutedEventArgs e)
        {
            if (focusPacket == null)
                return;

            byte[] bytes = null;
            Invoke(() =>
            {
                bytes = StringToByteArray(txtbox_Hex.Text.Replace(" ", ""));
            });

            if (bytes == null || bytes.Length < 1)
                return;

            var client = GetClientById(focusPacket.ID);
            if (client != null)
            {
                client.Relay.Send(bytes);
                ConsoleLog("Packet sent: " + focusPacket.Remote);
            }
        }

        private void btn_GetPlugins_Click(object sender, RoutedEventArgs e)
            => PopulatePlugins();

        #region Helpers

        private ClientMain GetClientById(int clientId)
            => server.Clients.FirstOrDefault(c => (c as ClientMain).ID == clientId) as ClientMain;

        private byte[] StringToByteArray(string hex)
        {
            var charsNum = hex.Length;
            var bytes = new byte[charsNum / 2];

            for (int i = 0; i < charsNum; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        private IEnumerable<Plugin> GetPlugins()
        {
            var files = Directory.GetFiles(Utils.AssemblyDirectory + @"\Plugins", "*.dll");
            foreach (var file in files)
            {
                Plugin pluginTemp = null;
                try
                {
                    var plugin = Assembly.LoadFrom(file);
                    var mainClass = plugin.GetTypes().FirstOrDefault
                        (x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Core)));

                    if (mainClass != null)
                    {
                        var methods = mainClass.GetMembers().Where(x => x.MemberType == MemberTypes.Method);
                        if (methods.Any(x => x.Name == "PluginRun") && methods.Any(x => x.Name == "PluginStop"))
                        {
                            pluginTemp = new Plugin
                            {
                                Name = System.IO.Path.GetFileName(file), Path = file
                            };
                        }
                    }
                }
                catch
                {
                    continue;
                }

                if (pluginTemp != null)
                    yield return pluginTemp;
            }
        }


        private void SendPacket(byte[] bytes)
        {
            foreach (var client in server.Clients)
            {
                (client as ClientMain).Relay.Send(bytes);
            }

            ConsoleLog("Packets sent!");
        }


        private void ExecutePlugin(string pluginPath)
        {
            try
            {
                var domainSetup = new AppDomainSetup() { PrivateBinPath = pluginPath };
                var domain = AppDomain.CreateDomain("Plugins", null, domainSetup);
                var asm = Assembly.GetExecutingAssembly();
                var instance = (PluginLoader)domain.CreateInstanceAndUnwrap(asm.FullName, typeof(PluginLoader).FullName);

                var proxy = new ProxyHandler();
                proxy.SendPacketProxy = new SendPacket(SendPacket);
                proxy.ConsoleLogProxy = new ConsoleLog(ConsoleLog);
                server.ClientRecv += (s, c, p) => { proxy.OnClientRecv(p.Buffer); };

                instance.Load(pluginPath, proxy);
            }
            catch (Exception e)
            {
                Utils.Log(e.Message + " " + e.StackTrace);
            }
        }

        #endregion
    }
}
