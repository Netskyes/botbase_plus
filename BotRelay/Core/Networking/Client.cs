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
        public IPEndPoint EndPoint { get; private set; }
        public DateTime ConnectedTime { get; private set; }
        public bool Connected { get; private set; }

        private Socket handle;
        private ClientRelay relay;
        private readonly Server parentServer;

        private ushort destPort;
        private string destAddress;


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


        public Client(Server server, Socket socket)
        {
            try
            {
                handle = socket;
                parentServer = server;

                ConnectedTime = DateTime.Now;
                EndPoint = (IPEndPoint)handle.RemoteEndPoint;


                readBuffer = new byte[server.BUFFER_SIZE];
                tempHeader = new byte[server.HEADER_SIZE];

                handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);
                OnClientState(true);

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
                    Utils.DebugLog("Zero bytes!");
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
                        }
                        else
                        {
                            relay.Send(readBuffer);
                        }
                        
                        break;
                    }


                    if (dataLen < parentServer.HEADER_SIZE)
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
                            ? (parentServer.HEADER_SIZE - tempHeaderOffset) : parentServer.HEADER_SIZE;

                        try
                        {
                            if (!appendHeader)
                            {
                                BuildRelayEndPoint(readBuffer);
                            }
                            else
                            {
                                Array.Copy(readBuffer, readOffset, tempHeader, tempHeaderOffset, headerLength);

                                BuildRelayEndPoint(tempHeader);

                                appendHeader = false;
                                tempHeaderOffset = 0;
                            }
                        }
                        catch (Exception)
                        {
                            Disconnect();
                            break;
                        }


                        ConnectRelay();
                        isHandshakeDone = true;

                        if ((bufferLen - headerLength) == 0)
                        {
                            break;
                        }


                        readOffset += headerLength;
                    }
                }
            }
        }





        private void ConnectRelay()
        {
            relay = new ClientRelay(parentServer, this);
            relay.Connect(destAddress, destPort);

            if (relay.Connected)
            {
                Utils.DebugLog("Relay connected: " + relay.EndPoint);
            }
            else
            {
                Disconnect();
            }
        }

        private void BuildRelayEndPoint(byte[] buffer)
        {
            ushort port = BitConverter.ToUInt16(buffer, 0);

            byte[] temp = new byte[4];
            Array.Copy(buffer, 2, temp, 0, temp.Length);

            uint address = (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(temp, 0));


            destPort = (ushort)IPAddress.NetworkToHostOrder((short)port);
            destAddress = (new IPAddress(address).ToString());
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
