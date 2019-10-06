using Aeon.Internal.Network;
using Aeon.Internal.Network.Data;

namespace Aeon.Internal
{
    public delegate void LogDelegate(string text, string tabName);
    public delegate void ClientPacketEx(Server server, Client client, Packet packet);
    public delegate void ServerPacketEx(Server server, Client client, Packet packet);
    public delegate void ClientPacket(int id, byte[] data);
    public delegate void ServerPacket(int id, byte[] data);
}
