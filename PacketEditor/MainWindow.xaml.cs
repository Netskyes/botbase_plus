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
using NetRelay.Network;
using System.Net;

namespace PacketEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<PacketController> TCPPacketItems
            { get => tcpPacketItems; }

        public ObservableCollection<PacketController> UDPPacketItems
            { get => udpPacketItems; }


        private Server server;
        private UdpServer udpServer;
        private const string processName = "Albion-Online.exe";
        private ObservableCollection<PacketController> tcpPacketItems = new ObservableCollection<PacketController>();
        private ObservableCollection<PacketController> udpPacketItems = new ObservableCollection<PacketController>();



        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            server = new Server();
            server.ServerState += OnServerState;
            // Main client events
            server.ClientSend += OnClientSend;
            server.ClientRecv += OnClientRecv;
            server.ClientState += OnClientState;

            server.Listen(8085);

            udpServer = new UdpServer();
            udpServer.BeginListenUdp(8085);
            udpServer.ClientRecv += OnUdpClientRecv;
            udpServer.ClientSend += OnUdpClientSend;
            udpServer.ClientState += OnUdpClientState;


            if (server.Listening && udpServer.Listening)
            {
                ConsoleLog($"Servers listening on: {server.Port}");
            }

            //BeginInjection(processName);
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

                udpPacketItems.Add(new PacketController
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

                udpPacketItems.Add(new PacketController
                {
                    ID = 1,
                    Client = null,
                    Remote = packet.EndPoint.ToString(),
                    Size = packet.Buffer.Length,
                    Content = Encoding.UTF8.GetString(packet.Buffer)
                });
            });
        }

        private void OnClientRecv(Server server, Client client, Packet packet)
        {
            var temp = client as ClientMain;
            if (temp is null || packet is null)
                return;

            Invoke(() =>
            {
                if (tcpPacketItems.Count > 100)
                    tcpPacketItems.Clear();

                tcpPacketItems.Add(new PacketController
                {
                    Client = client,
                    ID = temp.ID,
                    Remote = temp.EndPoint.ToString(),
                    Size = packet.Buffer.Length,
                    Content = Encoding.UTF8.GetString(packet.Buffer)
                });
            });
        }

        private void OnClientSend(Server server, Client client, Packet packet)
        {
        }

        private void OnClientState(Server server, Client client, bool isConnected)
        {
            if (isConnected)
            {
                ConsoleLog("Connected: " + client.EndPoint);
            }
        }


        private void OnServerState(bool isListening)
        {
        }



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

        private void ConsoleLog(string text)
        {
            Dispatcher.Invoke(() => txtbox_Console.AppendText(text + Environment.NewLine));
        }

        private void Invoke(Action a) => Dispatcher.Invoke(a);
    }
}
