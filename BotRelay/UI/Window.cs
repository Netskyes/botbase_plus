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
        private BotServer server;
        private UdpBotServer udpServer;

        private Dictionary<int, byte[]> sentPackets = new Dictionary<int, byte[]>();


        public Window()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            server = new BotServer();
            server.ClientState += OnClientState;
            server.ClientReceive += OnClientReceive;
            server.RelayState += OnRelayState;
            server.RelayReceive += OnRelayReceive;
            server.ServerState += OnServerState;
            server.PacketSent += OnPacketSent;

            server.Listen(8085);

            udpServer = new UdpBotServer();
            udpServer.Listen(8085);


            // Inject mapping
            BeginInjection("Albion-Online.exe");
        }

        
        #region Event Handlers

        private void OnServerState(Server s, bool isListening, ushort port)
        {
            if (isListening)
            {
                ConsoleLog("Server listening on port: " + port);
            }
        }

        private void OnClientState(Server s, Client c, bool isConnected)
        {
            if (isConnected)
            {
                ConsoleLog("Client [" + c.UID + "] connected: " + c.EndPoint + " @ " + c.ConnectedTime);
            }
            else
            {
                ConsoleLog("Client [" + c.UID + "] disconnected: " + c.EndPoint);
            }


            HandleRelays();
        }

        private void OnClientReceive(Server s, Client c, byte[] recv) 
            => AddToGrid(dtg_PacketsData, $"Client [{c.UID}]", c.EndPoint, recv.Length, Encoding.ASCII.GetString(recv));


        private void OnRelayState(Server s, ClientRelay rc, bool isConnected)
        {
            if (isConnected)
            {
                ConsoleLog("Relay [" + rc.UID + "] connected: " + rc.EndPoint + " @ " + rc.ConnectedTime);
            }
            else
            {
                ConsoleLog("Relay [" + rc.UID + "] disconnected: " + rc.EndPoint);
            }


            HandleRelays();
        }

        private void OnRelayReceive(Server s, ClientRelay rc, byte[] recv)
        {
            AddToGrid(dtg_PacketsData, $"Relay [{rc.UID}]", rc.EndPoint, recv.Length, Encoding.ASCII.GetString(recv));
        }


        private void OnPacketSent(Server s, Client c, byte[] recv) => AddPacketToGrid(c.ClientRelay, recv);

        #endregion


        public void ConsoleLog(string text)
        {
            Utils.InvokeOn(this, () => txtbox_Console.AppendText(text + Environment.NewLine));
        }

        private void BeginInjection(string processName)
        {
            Task.Run(() =>
            {
                bool result;

                do
                {
                    result = Imports.Inject(processName, @"D:\SoftwareDev\C#\BotRelay\Debug\Mapping.dll");
                    Utils.Sleep(10);
                }
                while (!result);


                ConsoleLog(processName + ": Injected to process!");
            });
        }

        private void HandleRelays()
        {
            var clients = server.Clients.Where
                (c => c.ClientRelay != null && c.ClientRelay.Connected);

            Utils.InvokeOn(this, () =>
            {
                dtg_Relays.Rows.Clear();

                if (!clients.Any())
                    return;

                foreach (var client in clients)
                {
                    if (client.ClientRelay == null)
                        continue;

                    var relay = client.ClientRelay;
                    int index = dtg_Relays.Rows.Add($"Relay [{relay.UID}]", relay.EndPoint);

                    try
                    {
                        dtg_Relays.Rows[index].Tag = relay.UID;
                    }
                    catch
                    {
                    }
                }
            });
        }

        private void btn_SendPacket_Click(object sender, EventArgs e)
        {
            Utils.InvokeOn(this, () =>
            {
                /*
                var packetRow = (dtg_Packets.SelectedRows.Count > 0) 
                    ? dtg_Packets.SelectedRows[0] : null;

                if (packetRow == null)
                    return;
                */

                var relayRow = (dtg_Relays.SelectedRows.Count > 0) 
                    ? dtg_Relays.SelectedRows[0] : null;

                if (relayRow == null)
                    return;


                //int packetTag = (int)packetRow.Tag;
                int relayUID = (int)relayRow.Tag;

                /*
                if (!sentPackets.ContainsKey(packetTag))
                    return;
                */

                var client = server.Clients.FirstOrDefault
                    (c => c.ClientRelay != null && c.ClientRelay.UID == relayUID);

                if (client == null)
                    return;


                var packet = BuildPacket(22, Encoding.ASCII.GetBytes("Hello, how are you?"));

                client.SendEndPacket(packet);
            });
        }

        private int AddToGrid(DataGridView dtg, params object[] data)
        {
            int index = -1;

            Utils.InvokeOn(this, () =>
            {
                if (dtg.Rows.Count > 100)
                    dtg.Rows.Clear();

                index = dtg.Rows.Add(data);

                if (index != -1)
                {
                    dtg.FirstDisplayedScrollingRowIndex = index;
                }
            });

            return index;
        }

        private void AddPacketToGrid(ClientRelay relay, byte[] recv)
        {
            if (relay == null || !IsPacketValid(recv))
                return;

            Utils.InvokeOn(this, () =>
            {
                byte[] tempLen = new byte[2];

                try
                {
                    Array.Copy(recv, 0, tempLen, 0, tempLen.Length);
                }
                catch
                {
                    return;
                }

                short packetLen = BitConverter.ToInt16(tempLen, 0);
                

                byte[] tempText = new byte[packetLen - 1];
                
                try
                {
                    Array.Copy(recv, 3, tempText, 0, tempText.Length);
                }
                catch
                {
                    return;
                }

                string packetContent = Encoding.ASCII.GetString(tempText);

                // Operation
                byte opcode = recv[2];


                int index = AddToGrid(dtg_Packets,
                    $"Relay [{relay.UID}]", relay.EndPoint, opcode, recv.Length + "/" + (packetLen), packetContent);

                int packetTag = sentPackets.Count;
                dtg_Packets.Rows[index].Tag = packetTag;

                sentPackets.Add(packetTag, recv);
            });
        }

        private void dtg_Packets_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Utils.InvokeOn(this, () =>
            {
                var row = dtg_Packets.CurrentRow;

                if (row == null)
                    return;

                if (!sentPackets.ContainsKey((int)row.Tag))
                    return;


                var packet = sentPackets[(int)row.Tag];

                txtbox_PacketContents.Clear();
                txtbox_PacketContents.Text = Encoding.UTF8.GetString(packet);
            });
        }


        
        private bool IsPacketValid(byte[] packet) // [2] Length, [1] OpCode, [Length] Structure
        {
            if (packet == null || packet.Length < 3)
                return false;
            
            
            short length = 0;
            
            try
            {
                byte[] tempLen = new byte[2];
                Array.Copy(packet, 0, tempLen, 0, tempLen.Length);

                length = BitConverter.ToInt16(tempLen, 0);
            }
            catch (Exception)
            {
            }

            // -2 len bytes
            return ((packet.Length - 2) >= length);
        }

        private byte[] BuildPacket(byte opcode, byte[] payload)
        {
            if (payload == null)
                return null;


            byte[] temp = new byte[3 + payload.Length];

            try
            {
                Array.Copy(BitConverter.GetBytes((short)payload.Length), 0, temp, 0, 2);
                temp[2] = opcode;

                Array.Copy(payload, 0, temp, 3, payload.Length);
            }
            catch
            {
            }

            return temp;
        }
    }
}
