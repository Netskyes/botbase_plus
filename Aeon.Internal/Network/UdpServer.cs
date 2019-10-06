using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Aeon.Internal.Network
{
    using Data;

    public class UdpServer : Client
    {
        public bool Listening { get; private set; }
        public Dictionary<IPEndPoint, IPEndPoint> UdpLinks => links;

        private Dictionary<IPEndPoint, IPEndPoint> links = new Dictionary<IPEndPoint, IPEndPoint>();

        public UdpServer()
        {
            BUFFER_SIZE = (1024 * 1024) * 50; // 50MB
        }
        
        public void Listen(ushort port) 
            => BeginListenUdp(port);

        protected override void OnClientState(bool isConnected)
        {
            Listening = isConnected;
            base.OnClientState(isConnected);
        }

        protected override void AsyncRecvProcess(Packet packet)
        {
            if (IsPacketAddress(packet.Buffer))
            {
                var destination = GetEndPoint(packet.Buffer);
                
                lock (links)
                {
                    AddLink(packet.EndPoint, destination);
                }

                return;
            }


            IPEndPoint endPoint = null;

            lock (links)
            {
                if (!links.ContainsKey(packet.EndPoint))
                    return;

                endPoint = links[packet.EndPoint];
            }

            Send(ProcessPacket
                (new Packet { Buffer = packet.Buffer, EndPoint = endPoint, UdpPacket = true }));
        }

        private void AddLink(IPEndPoint ep1, IPEndPoint ep2)
        {
            if (links.ContainsKey(ep1))
                links.Remove(ep1);
            if (links.ContainsKey(ep2))
                links.Remove(ep2);

            links.Add(ep1, ep2);
            links.Add(ep2, ep1);
        }

        private Packet ProcessPacket(Packet packet)
        {
            byte[] buffer = packet.Buffer;
            // Do whatever with packet before its sent to destination.

            return packet;
        }


        private IPEndPoint GetEndPoint(byte[] buffer)
        {
            var port = GetDestPort(buffer);
            var address = GetDestAddress(buffer);

            return new IPEndPoint(new IPAddress(address), port);
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
    }
}
