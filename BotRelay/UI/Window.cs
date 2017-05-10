using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BotRelay.Core.Networking;
using System.Net;
using System.Net.Sockets;

namespace BotRelay
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
        }

        public void ConsoleLog(string text)
        {
            Utils.InvokeOn(this, () => txtbox_Console.AppendText(text + Environment.NewLine));
        }


        private BotServer server;
        private UdpBotServer udpServer;

        private void Window_Load(object sender, EventArgs e)
        {
            server = new BotServer();
            server.ServerState += OnServerState;
            server.ClientState += OnClientState;
            server.ClientReceive += OnClientReceive;
            server.Listen(8085);

            udpServer = new UdpBotServer();
            udpServer.Listen(8085);


            Task.Run(() =>
            {
                bool result = false;

                while (!result)
                {
                    result = Imports.Inject("Steam.exe", @"D:\SoftwareDev\C#\BotRelay\Debug\Mapping.dll");
                    Thread.Sleep(10);
                }

                ConsoleLog("Injected!");
            });
        }

        private void OnClientReceive(Server s, Client c, byte[] recv)
        {
            Utils.InvokeOn(this, () => dtg_Packets.Rows.Add(recv.Length, Encoding.UTF8.GetString(recv)));
        }

        private void OnClientState(Server s, Client c, bool isConnected)
        {
            if (isConnected)
            {
                ConsoleLog("Connected: " + c.EndPoint +  " @ " + c.ConnectedTime);
            }
            else
            {
                ConsoleLog("Disconnected: " + c.EndPoint);
                ConsoleLog("Total connected: " + server.ConnectedClients.Length);
            }
        }

        private void OnServerState(Server s, bool isListening, ushort port)
        {
            if (isListening)
            {
                ConsoleLog("Server listening on port: " + port);
            }
        }
    }
}
