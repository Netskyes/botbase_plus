using System;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NetRelay.Network
{
    public class Client
    {
        public Socket Socket { get => handle; }
        public IPEndPoint EndPoint { get; private set; }
        public bool Connected { get; private set; }
        public DateTime ConnectedTime { get; private set; }

        public int BUFFER_SIZE { get; set; }

        private Socket handle;
        private Server parent;
        private byte[] readBuffer;

        #region Buffers

        private Queue<Packet> sendBuffers = new Queue<Packet>();
        private readonly object sendingPacketsLock = new object();
        private bool sendingPackets;

        private Queue<Packet> readBuffers = new Queue<Packet>();
        private readonly object readingPacketsLock = new object();
        private bool readingPackets;

        #endregion

        #region Event Handlers

        public event ClientStateEventHandler ClientState;
        public delegate void ClientStateEventHandler(Client client, bool isConnected);

        protected virtual void OnClientState(bool isConnected)
        {
            if (Connected == isConnected)
                return;

            Connected = isConnected;
            ClientState?.Invoke(this, isConnected);
        }

        public event ClientRecvEventHandler ClientRecv;
        public delegate void ClientRecvEventHandler(Client client, Packet packet);

        protected virtual void OnClientRecv(Packet packet)
        {
            if (packet is null)
                return;

            ClientRecv?.Invoke(this, packet);
        }

        public event ClientSendEventHandler ClientSend;
        public delegate void ClientSendEventHandler(Client client, Packet packet);

        protected virtual void OnClientSend(Packet packet)
        {
            if (packet is null)
                return;

            ClientSend?.Invoke(this, packet);
        }

        #endregion


        public Client()
        {
        }

        public void BeginListen(Server server, Socket socket)
        {
            try
            {
                if (handle != null)
                    DisposeHandle();

                handle = socket;
                parent = server;

                ConnectedTime = DateTime.Now;
                EndPoint = (IPEndPoint)socket.RemoteEndPoint;

                OnClientState(true);


                readBuffer = new byte[BUFFER_SIZE];

                handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void BeginListenUdp(ushort port)
        {
            try
            {
                if (handle != null)
                    DisposeHandle();

                ConnectedTime = DateTime.Now;
                EndPoint = null;

                handle = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                handle.Bind(new IPEndPoint(IPAddress.Any, port));

                OnClientState(true);


                readBuffer = new byte[BUFFER_SIZE];

                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                handle.BeginReceiveFrom(readBuffer, 0, readBuffer.Length, SocketFlags.None, ref remote, AsyncUdpReceive, null);

            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void Connect(IPEndPoint endPoint)
            => Connect(endPoint.Address.ToString(), (ushort)endPoint.Port);

        public void Connect(string host, ushort port)
        {
            try
            {
                if (handle != null)
                    DisposeHandle();

                handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                handle.Connect(host, port);

                if (handle.Connected)
                {
                    ConnectedTime = DateTime.Now;
                    EndPoint = (IPEndPoint)handle.RemoteEndPoint;

                    OnClientState(true);


                    readBuffer = new byte[BUFFER_SIZE];

                    handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void Send(byte[] bytes)
            => Send(new Packet { Buffer = bytes });

        public void Send(byte[] bytes, IPEndPoint endPoint)
            => Send(new Packet { Buffer = bytes, EndPoint = EndPoint, UdpPacket = true });

        public void Send(Packet packet)
        {
            if (!Connected || packet == null)
                return;


            OnClientSend(packet);

            lock (sendBuffers)
            {
                sendBuffers.Enqueue(packet);

                lock (sendingPacketsLock)
                {
                    if (sendingPackets)
                        return;

                    sendingPackets = true;
                }

                ThreadPool.QueueUserWorkItem(SendProcess);
            }
        }

        public void Disconnect()
        {
            if (handle != null) DisposeHandle();
            
            OnClientState(false);
        }


        private void AsyncUdpReceive(IAsyncResult result)
        {
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            int bytesTransfered;

            try
            {
                bytesTransfered = handle.EndReceiveFrom(result, ref remote);

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

            var packet = new Packet { Buffer = received, EndPoint = (IPEndPoint)remote, UdpPacket = true };
            OnClientRecv(packet);


            lock (readBuffers)
            {
                readBuffers.Enqueue(packet);
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
                handle.BeginReceiveFrom(readBuffer, 0, readBuffer.Length, SocketFlags.None, ref remote, AsyncUdpReceive, null);
            }
            catch (ObjectDisposedException)
            {
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

            var packet = new Packet { Buffer = received };
            OnClientRecv(packet);


            lock (readBuffers)
            {
                readBuffers.Enqueue(packet);
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
                Packet packet;

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

                    packet = readBuffers.Dequeue();
                }

                if (packet != null)
                {
                    AsyncProcess(packet);
                }
            }
        }

        protected virtual void AsyncProcess(Packet packet)
        {
        }

        private void SendProcess(object state)
        {
            while (true)
            {
                if (!Connected)
                {
                    SendCleanup(false);
                    return;
                }

                Packet packet;

                lock (sendBuffers)
                {
                    if (sendBuffers.Count == 0)
                    {
                        SendCleanup(false);
                        return;
                    }

                    packet = sendBuffers.Dequeue();
                }

                if (packet == null)
                    continue;

                try
                {
                    if (!packet.UdpPacket)
                    {
                        handle.Send(packet.Buffer);
                    }
                    else
                    {
                        handle.SendTo(packet.Buffer, packet.EndPoint);
                    }
                }
                catch (Exception)
                {
                    Disconnect();
                    SendCleanup(true);
                    return;
                }
            }
        }

        private void SendCleanup(bool clear)
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

        private void DisposeHandle()
        {
            handle.Close();
            handle = null;
        }
    }
}
