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
    public class ClientRelay
    {
        public int UID { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public DateTime ConnectedTime { get; private set; }
        public bool Connected { get; private set; }
        

        private Socket handle;
        private Client client;
        private Server parentServer;


        private byte[] readBuffer;


        private readonly Queue<byte[]> sendBuffers = new Queue<byte[]>();
        private readonly object sendingPacketsLock = new object();
        private bool sendingPackets;


        /// <summary>
        /// When relay connection state changes.
        /// </summary>
        public event RelayStateEventHandler RelayState;
        public delegate void RelayStateEventHandler(ClientRelay rc, bool isConnected);

        private void OnRelayState(bool isConnected)
        {
            if (Connected == isConnected)
                return;

            Connected = isConnected;
            RelayState?.Invoke(this, isConnected);
        }


        /// <summary>
        /// When relay receives data.
        /// </summary>
        public event RelayReceiveEventHandler RelayReceive;
        public delegate void RelayReceiveEventHandler(ClientRelay rc, byte[] recv);

        private void OnRelayReceive(byte[] recv) => RelayReceive?.Invoke(this, recv);


        public ClientRelay(Server s, Client c, int uid)
        {
            client = c;
            parentServer = s;
            UID = uid;

            readBuffer = new byte[s.BUFFER_SIZE];
        }

        public void Connect(string host, ushort port)
        {
            try
            {
                handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                handle.Connect(host, port);

                if (handle.Connected)
                {
                    ConnectedTime = DateTime.Now;
                    EndPoint = (IPEndPoint)handle.RemoteEndPoint;

                    OnRelayState(true);


                    handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);
                    
                }
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
                Array.Copy(readBuffer, 0, received, 0, received.Length);
            }
            catch (Exception)
            {
                Disconnect();
                return;
            }

            OnRelayReceive(received);


            if (client.Connected)
            {
                client.Send(received);
            }
            else
            {
                Disconnect();
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
            }

            OnRelayState(false);
        }
    }
}
