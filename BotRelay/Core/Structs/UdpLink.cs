using System.Net;

namespace BotRelay.Core.Structs
{
    public struct UdpLink
    {
        public IPEndPoint StartPoint { get; set; }
        public IPEndPoint EndPoint { get; set; }
    }
}
