using System.Net;

namespace NetRelay.Network
{
    public class Packet
    {
        public byte[] Buffer { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public bool UdpPacket { get; set; }
    }
}
