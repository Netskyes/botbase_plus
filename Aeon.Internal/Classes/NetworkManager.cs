using System;
using Aeon.Internal.Network;
using Aeon.Internal.Network.Data;

namespace Aeon.Internal
{
    sealed class NetworkManager
    {
        internal ClientPacket ClientPacket;
        internal ServerPacket ServerPacket;
        internal ClientPacketEx ClientPacketEx;
        internal ServerPacketEx ServerPacketEx;
        
        internal Server Server { get; set; }
        internal UdpServer UdpServer { get; set; }

        public NetworkManager()
        {
            Server = new Server();
            Server.ClientRecv += Server_ClientRecv;
            Server.Listen(8089);

            UdpServer = new UdpServer();
            UdpServer.Listen(8089);
            UdpServer.ClientRecv += UdpServer_ClientRecv;
        }

        private void UdpServer_ClientRecv(Client client, Packet packet)
        {
            Server_ClientRecv(null, client, packet);
        }

        private void Server_ClientRecv(Server server, Client client, Packet packet)
        {
            var remoteEndPoint = (packet.UdpPacket) ? packet.EndPoint : client.EndPoint;
            if (remoteEndPoint.Address.ToString() == "127.0.0.1")
            {
                ClientPacketEx?.Invoke(server, client, packet);
                OnClientPacket(packet);
            }
            else
            {
                ServerPacketEx?.Invoke(server, client, packet);
                OnServerPacket(packet);
            }
        }

        private void OnClientPacket(Packet packet)
        {
            ClientPacket?.Invoke(0, packet.Buffer);
        }

        private void OnServerPacket(Packet packet)
        {
            ServerPacket?.Invoke(0, packet.Buffer);
        }

        public bool IsListening()
        {
            return Server != null && UdpServer != null && Server.Listening && UdpServer.Listening;
        }
    }
}
