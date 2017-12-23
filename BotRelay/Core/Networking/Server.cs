using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace BotRelay.Core.Networking
{
    public class Server
    {
        public bool Listening { get; private set; }
        public ushort Port { get; private set; }

        public long BytesReceived { get; set; }

            
        protected Server()
        {
            clients = new List<Client>();
        }

        private Socket handle;
        private SocketAsyncEventArgs item;

        protected bool ProcessDisconnect { get; set; }


        private List<Client> clients;
        private readonly object clientsLock = new object();

        public Client[] Clients
        {
            get
            {
                lock (clientsLock)
                {
                    return clients.ToArray();
                }
            }
        }


        /// <summary>
        /// When server listening state changes.
        /// </summary>
        public event ServerStateEventHandler ServerState;
        public delegate void ServerStateEventHandler(Server s, bool isListening, ushort port);

        private void OnServerState(bool isListening)
        {
            if (Listening == isListening)
                return;

            Listening = isListening;
            ServerState?.Invoke(this, isListening, Port);
        }


        /// <summary>
        /// When client connection state changes.
        /// </summary>
        public event ClientStateEventHandler ClientState;
        public delegate void ClientStateEventHandler(Server s, Client c, bool isConnected);

        private void OnClientState(Client c, bool isConnected)
        {
            if (!isConnected)
            {
                RemoveClient(c);
            }

            ClientState?.Invoke(this, c, isConnected);
        }


        /// <summary>
        /// When client receives data.
        /// </summary>
        public event ClientReceiveStateHandler ClientReceive;
        public delegate void ClientReceiveStateHandler(Server s, Client c, byte[] recv);

        private void OnClientReceive(Client c, byte[] recv)
        {
            if (recv.Length > 0)
            {
                ClientReceive?.Invoke(this, c, recv);
            }
        }


        /// <summary>
        /// When packet is sent through relay.
        /// </summary>
        public event PacketSentEventHandler PacketSent;
        public delegate void PacketSentEventHandler(Server s, Client c, byte[] recv);

        private void OnPacketSent(Client c, byte[] recv) => PacketSent?.Invoke(this, c, recv);


        /// <summary>
        /// When relay connection state changes.
        /// </summary>
        public event RelayStateEventHandler RelayState;
        public delegate void RelayStateEventHandler(Server s, ClientRelay rc, bool isConnected);

        private void OnRelayState(ClientRelay rc, bool isConnected) => RelayState?.Invoke(this, rc, isConnected);


        /// <summary>
        /// When relay receives data.
        /// </summary>
        public event RelayReceiveEventHandler RelayReceive;
        public delegate void RelayReceiveEventHandler(Server s, ClientRelay rc, byte[] recv);

        private void OnRelayReceive(ClientRelay rc, byte[] recv)
        {
            if (recv.Length > 0)
            {
                RelayReceive?.Invoke(this, rc, recv);
            }
        }


        public int BUFFER_SIZE { get { return 1024 * 1024; } } // 1MB
        public int MAX_PACKET_SIZE { get { return (1024 * 1024) * 10; } } // 10MB


        public void Listen(ushort port)
        {
            Port = port;

            try
            {
                if (Listening)
                    return;

                if (handle != null)
                {
                    DisposeHandle();
                }

                handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                handle.Bind(new IPEndPoint(IPAddress.Any, port));
                handle.Listen(1000);

                ProcessDisconnect = false;
                OnServerState(true);


                if (item != null)
                    DisposeItem();

                item = new SocketAsyncEventArgs();
                item.Completed += AcceptClient;

                if (!handle.AcceptAsync(item)) AcceptClient(null, item);

            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10048)
                {
                    // Port already in use
                }
                else
                {
                    // Some unexpected socket error
                }

                Disconnect();
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void AcceptClient(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                do
                {
                    switch (e.SocketError)
                    {
                        case SocketError.Success:
                            var client = new Client(this, e.AcceptSocket);

                            AddClient(client);
                            OnClientState(client, true);
                            break;

                        case SocketError.ConnectionReset:
                            break;

                        default:
                            throw new Exception("Socket Error");
                    }

                    e.AcceptSocket = null;
                }
                while (!handle.AcceptAsync(e));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                Disconnect();
            }
        }


        private void AddClient(Client client)
        {
            lock (clientsLock)
            {
                client.ClientState += OnClientState;
                client.ClientReceive += OnClientReceive;
                client.RelayState += OnRelayState;
                client.RelayReceive += OnRelayReceive;
                client.PacketSent += OnPacketSent;
                clients.Add(client);
            }
        }

        private void RemoveClient(Client client)
        {
            lock (clientsLock)
            {
                client.ClientState -= OnClientState;
                client.ClientReceive -= OnClientReceive;
                client.RelayState -= OnRelayState;
                client.RelayReceive -= OnRelayReceive;
                client.PacketSent -= OnPacketSent;
                clients.Remove(client);
            }
        }

        private void DisposeHandle()
        {
            handle.Close();
            handle = null;
        }

        private void DisposeItem()
        {
            item.Dispose();
            item = null;
        }

        private void Disconnect()
        {
            if (ProcessDisconnect)
                return;

            ProcessDisconnect = true;


            if (handle != null)
            {
                DisposeHandle();
            }

            if (item != null)
            {
                DisposeItem();
            }

            lock (clientsLock)
            {
                while (clients.Count != 0)
                {
                    try
                    {
                        clients[0].Disconnect();
                        clients[0].ClientState -= OnClientState;
                        clients[0].ClientReceive -= OnClientReceive;
                        clients[0].RelayState -= OnRelayState;
                        clients[0].RelayReceive -= OnRelayReceive;
                        clients[0].PacketSent -= OnPacketSent;
                        clients.RemoveAt(0);
                    }
                    catch
                    {
                    }
                }
            }


            ProcessDisconnect = false;
            OnServerState(false);
        }
    }
}
