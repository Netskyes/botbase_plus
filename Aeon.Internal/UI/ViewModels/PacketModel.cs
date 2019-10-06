using Aeon.Internal.Network;
using System.Net;

namespace Aeon.Internal.UI
{
    using Enums;

    public sealed class PacketModel
    {
        public Client Client { get; set; }
        public Server Server { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public int ID { get; set; }
        public int Length { get; set; }
        public byte[] Bytes { get; set; }
        public PacketType Type { get; set; }
        public PacketSource Source { get; set; }

        public string TypeHelp { get; set; }
        public string Remote
        {
            get { return EndPoint?.ToString() ?? "Unknown"; }
        }
    }
}
