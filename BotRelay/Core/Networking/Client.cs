using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BotRelay.Core.Networking
{
    public class Client
    {
        public int UID { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public DateTime ConnectedTime { get; private set; }
        public bool Connected { get; private set; }
        public ClientRelay ClientRelay { get { return relay; } }

        // Relay
        private ushort destPort;
        private string destAddress;


        private Socket handle;
        private ClientRelay relay;
        private readonly Server parentServer;


        private bool appendHeader;
        private bool isHandshakeDone;

        private byte[] readBuffer;
        private byte[] tempHeader;
        private int dataLen;
        private int readOffset;
        private int tempHeaderOffset;

        
        private readonly Queue<byte[]> readBuffers = new Queue<byte[]>();
        private readonly object readingPacketsLock = new object();
        private bool readingPackets;

        private readonly Queue<byte[]> sendBuffers = new Queue<byte[]>();
        private readonly object sendingPacketsLock = new object();
        private bool sendingPackets;


        /// <summary>
        /// When client connection state changes.
        /// </summary>
        public event ClientStateEventHandler ClientState;
        public delegate void ClientStateEventHandler(Client c, bool isConnected);

        private void OnClientState(bool isConnected)
        {
            if (Connected == isConnected)
                return;

            Connected = isConnected;
            ClientState?.Invoke(this, isConnected);
        }


        /// <summary>
        /// When client receives data.
        /// </summary>
        public event ClientReceiveEventHandler ClientReceive;
        public delegate void ClientReceiveEventHandler(Client c, byte[] recv);

        private void OnClientReceive(byte[] recv) => ClientReceive?.Invoke(this, recv);


        /// <summary>
        /// When packet is sent through relay.
        /// </summary>
        public event PacketSentEventHandler PacketSent;
        public delegate void PacketSentEventHandler(Client c, byte[] recv);

        private void OnPacketSent(byte[] recv) => PacketSent?.Invoke(this, recv);


        /// <summary>
        /// When relay connection state changes.
        /// </summary>
        public event RelayStateEventHandler RelayState;
        public delegate void RelayStateEventHandler(ClientRelay rc, bool isConnected);

        private void OnRelayState(ClientRelay rc, bool isConnected) => RelayState?.Invoke(rc, isConnected);


        /// <summary>
        /// When relay receives data.
        /// </summary>
        public event RelayReceiveEventHandler RelayReceive;
        public delegate void RelayReceiveEventHandler(ClientRelay rc, byte[] recv);

        private void OnRelayReceive(ClientRelay rc, byte[] recv) => RelayReceive?.Invoke(rc, recv);


        private int HEADER_SIZE { get { return 6; } } // 6B


        public Client(Server server, Socket socket)
        {
            try
            {
                handle = socket;
                parentServer = server;

                UID = Utils.RandomNum();
                ConnectedTime = DateTime.Now;
                EndPoint = (IPEndPoint)handle.RemoteEndPoint;
                
                OnClientState(true);


                readBuffer = new byte[server.BUFFER_SIZE];
                tempHeader = new byte[HEADER_SIZE];

                handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);

            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void AsyncReceive(IAsyncResult result)
        {
            int bytesTransfered;

            try
            {
                bytesTransfered = handle.EndReceive(result);

                if (bytesTransfered <= 0)
                {
                    throw new Exception("No bytes transfered!");
                }
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception)
            {
                Disconnect();
                return;
            }

            parentServer.BytesReceived += bytesTransfered;

            byte[] received = new byte[bytesTransfered];

            try
            {
                Array.Copy(readBuffer, received, received.Length);
            }
            catch (Exception)
            {
                Disconnect();
                return;
            }

            OnClientReceive(received);


            lock (readBuffers)
            {
                readBuffers.Enqueue(received);
            }

            lock (readingPacketsLock)
            {
                if (!readingPackets)
                {
                    readingPackets = true;
                    ThreadPool.QueueUserWorkItem(AsyncProcess);
                }
            }


            try
            {
                handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void AsyncProcess(object state)
        {
            while (true)
            {
                byte[] readBuffer;

                lock (readBuffers)
                {
                    if (readBuffers.Count == 0)
                    {
                        lock (readingPacketsLock)
                        {
                            readingPackets = false;
                        }

                        return;
                    }

                    readBuffer = readBuffers.Dequeue();
                }

                int bufferLen = readBuffer.Length;
                dataLen += bufferLen;


                while (true)
                {
                    if (isHandshakeDone)
                    {
                        if (!relay.Connected)
                        {
                            Disconnect();
                            break;
                        }

                        if (readOffset > 0)
                        {
                            byte[] tempBuffer = new byte[bufferLen - readOffset];

                            try
                            {
                                Array.Copy(readBuffer, readOffset, tempBuffer, 0, tempBuffer.Length);
                            }
                            catch (Exception)
                            {
                                Disconnect();
                                break;
                            }

                            relay.Send(tempBuffer);
                            readOffset = 0;

                            // Packet sent
                            OnPacketSent(tempBuffer);
                        }
                        else
                        {
                            relay.Send(readBuffer);

                            // Packet sent
                            OnPacketSent(readBuffer);
                        }
                        
                        break;
                    }


                    if (dataLen < HEADER_SIZE)
                    {
                        try
                        {
                            Array.Copy(readBuffer, readOffset, tempHeader, tempHeaderOffset, bufferLen);
                        }
                        catch (Exception)
                        {
                            Disconnect();
                            break;
                        }

                        tempHeaderOffset += bufferLen;
                        appendHeader = true;

                        break;
                    }
                    else
                    {
                        int headerLength = (appendHeader) 
                            ? (HEADER_SIZE - tempHeaderOffset) : HEADER_SIZE;

                        try
                        {
                            if (!appendHeader)
                            {
                                GetRelayEndPoint(readBuffer);
                            }
                            else
                            {
                                Array.Copy(readBuffer, readOffset, tempHeader, tempHeaderOffset, headerLength);

                                GetRelayEndPoint(tempHeader);
                            }
                        }
                        catch (Exception)
                        {
                            Disconnect();
                            break;
                        }

                        isHandshakeDone = true;
                        

                        ConnectRelay();

                        if ((bufferLen - headerLength) == 0)
                            break;


                        readOffset += headerLength;
                    }
                }
            }
        }


        private void ConnectRelay()
        {
            relay = new ClientRelay(parentServer, this, UID);
            relay.RelayState += OnRelayState;
            relay.RelayReceive += OnRelayReceive;

            // Establish connection
            relay.Connect(destAddress, destPort);
        }

        private void GetRelayEndPoint(byte[] buffer)
        {
            ushort port = BitConverter.ToUInt16(buffer, 0);
            destPort = (ushort)IPAddress.NetworkToHostOrder((short)port);

            byte[] temp = new byte[4];
            Array.Copy(buffer, 2, temp, 0, temp.Length);

            uint address = (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(temp, 0));
            destAddress = (new IPAddress(address).ToString());
        }

        public void SendEndPacket(byte[] packet)
        {
            if (relay == null || !relay.Connected || packet == null)
                return;

            relay.Send(packet);
        }


        public void Send(byte[] packet)
        {
            if (!Connected || packet == null)
                return;

            lock (sendBuffers)
            {
                sendBuffers.Enqueue(packet);

                lock (sendingPacketsLock)
                {
                    if (sendingPackets)
                        return;

                    sendingPackets = true;
                }

                ThreadPool.QueueUserWorkItem(Send);
            }
        }

        private void Send(object state)
        {
            while (true)
            {
                if (!Connected)
                {
                    SendCleanup();
                    return;
                }

                byte[] packet;

                lock (sendBuffers)
                {
                    if (sendBuffers.Count == 0)
                    {
                        SendCleanup();
                        return;
                    }

                    packet = sendBuffers.Dequeue();
                }

                try
                {
                    handle.Send(packet);
                }
                catch (Exception)
                {
                    Disconnect();
                    SendCleanup(true);
                    return;
                }
            }
        }

        private void SendCleanup(bool clear = false)
        {
            lock (sendingPacketsLock)
            {
                sendingPackets = false;
            }

            if (clear)
            {
                lock (sendBuffers)
                {
                    sendBuffers.Clear();
                }
            }
        }


        public void Disconnect()
        {
            if (handle != null)
            {
                handle.Close();
                handle = null;
                tempHeaderOffset = 0;
                readOffset = 0;
                dataLen = 0;
            }

            OnClientState(false);
        }
    }
}
