using System.Net;

namespace BotRelay.Core.Structs
{
    public struct UdpPacket
    {
        public EndPoint Address { get; set; }
        public byte[] Buffer { get; set; }
    }
}
