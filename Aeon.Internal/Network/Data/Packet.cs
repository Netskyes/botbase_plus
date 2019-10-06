using System.Net;

namespace Aeon.Internal.Network.Data
{
    public class Packet
    {
        public byte[] Buffer { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public bool UdpPacket { get; set; }
    }
}
