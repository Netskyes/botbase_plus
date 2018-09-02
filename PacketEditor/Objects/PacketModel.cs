using System;
using NetRelay.Network;

namespace PacketEditor.Objects
{
    public class PacketModel
    {
        public Client Client { get; set; }
        public int ID { get; set; }
        public string Type { get; set; }
        public string Remote { get; set; }
        public int Size { get; set; }
        public string Content { get; set; }
        public byte[] Bytes { get; set; }
    }
}
