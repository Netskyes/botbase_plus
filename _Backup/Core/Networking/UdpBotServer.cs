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
    using Structs;

    public class UdpBotServer
    {
        private Socket handle;


        private byte[] readBuffer;

        private List<UdpLink> udpLinks = new List<UdpLink>();
        private readonly object linksLock = new object();

        private readonly Queue<UdpPacket> readBuffers = new Queue<UdpPacket>();
        private readonly object readingPacketsLock = new object();
        private bool readingPackets;

        private readonly Queue<UdpPacket> sendBuffers = new Queue<UdpPacket>();
        private readonly object sendingPacketsLock = new object();
        private bool sendingPackets;


        /// <summary>
        /// When packet is received.
        /// </summary>
        public event ClientReceiveEventHandler PacketRecv;
        public delegate void ClientReceiveEventHandler(EndPoint ep, byte[] recv);

        private void OnPacketRecv(EndPoint ep, byte[] recv) => PacketRecv?.Invoke(ep, recv);
        

        public UdpBotServer()
        {
            readBuffer = new byte[(1024 * 1024) * 4]; // 4MB
        }

        public void Listen(ushort port)
        {
            try
            {
                handle = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                handle.Bind(new IPEndPoint(IPAddress.Any, port));

                BeginReceiveFrom();
            }
            catch (Exception)
            {
            }
        }

        private void BeginReceiveFrom()
        {
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            handle.BeginReceiveFrom(readBuffer, 0, readBuffer.Length, SocketFlags.None, ref remote, AsyncReceive, null);
        }


        private void AsyncReceive(IAsyncResult result)
        {
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            int bytesTransfered = 0;

            try
            {
                bytesTransfered = handle.EndReceiveFrom(result, ref remote);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception)
            {
            }

            

            if (bytesTransfered > 0)
            {
                byte[] received = new byte[bytesTransfered];

                try
                {
                    Array.Copy(readBuffer, received, received.Length);
                }
                catch (Exception)
                {
                }

                OnPacketRecv(remote, received);


                lock (readBuffers)
                {
                    readBuffers.Enqueue(new UdpPacket() { Address = remote, Buffer = received });
                }

                lock (readingPacketsLock)
                {
                    if (!readingPackets)
                    {
                        readingPackets = true;
                        ThreadPool.QueueUserWorkItem(AsyncProcess);
                    }
                }
            }


            try
            {
                BeginReceiveFrom();
            }
            catch (Exception)
            {
            }
        }


        private void AsyncProcess(object state)
        {
            while (true)
            {
                UdpPacket packet;

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


                if (IsPacketAddress(packet.Buffer))
                {
                    var port = GetDestPort(packet.Buffer);
                    var address = GetDestAddress(packet.Buffer);

                    var destAddress = new IPEndPoint(new IPAddress(address), port);


                    lock (linksLock)
                    {
                        udpLinks.Add(new UdpLink() { StartPoint = ((IPEndPoint)packet.Address), EndPoint = destAddress });
                    }

                    continue;
                }


                EndPoint remoteEndPoint = null;

                lock (linksLock)
                {
                    var address = ((IPEndPoint)packet.Address);

                    var link = udpLinks.FirstOrDefault
                        (l => l.StartPoint.Equals(address) || l.EndPoint.Equals(address));


                    remoteEndPoint = (link.StartPoint.Equals(address)) ? link.EndPoint : link.StartPoint;
                }


                // Relay to destination
                Send(new UdpPacket() { Buffer = (packet.Buffer), Address = (remoteEndPoint) });
            }
        }


        private ushort GetDestPort(byte[] buffer)
        {
            ushort port = BitConverter.ToUInt16(buffer, 2);
            return (ushort)IPAddress.NetworkToHostOrder((short)port);
        }

        private uint GetDestAddress(byte[] buffer)
        {
            byte[] temp = new byte[4];

            try
            {
                Array.Copy(buffer, 4, temp, 0, temp.Length);
            }
            catch
            {
                return 0;
            }

            return (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(temp, 0));
        }

        private bool IsPacketAddress(byte[] buffer)
        {
            ushort opcode;

            try
            {
                opcode = BitConverter.ToUInt16(buffer, 0);
                opcode = (ushort)IPAddress.NetworkToHostOrder((short)opcode);
            }
            catch
            {
                return false;
            }

            return (opcode == 1337);
        }



        public void Send(UdpPacket packet)
        {
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
                UdpPacket packet;

                lock (sendBuffers)
                {
                    if (sendBuffers.Count == 0)
                    {
                        lock (sendingPacketsLock)
                        {
                            sendingPackets = false;
                        }

                        return;
                    }
                    
                    packet = sendBuffers.Dequeue();
                }

                try
                {
                    handle.SendTo(packet.Buffer, packet.Address);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
