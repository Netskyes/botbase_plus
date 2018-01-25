using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetRelay.Network;
using System.ComponentModel;

namespace PacketEditor
{
    public class PacketController
    {
        public Client Client { get; set; }
        public int ID { get; set; }
        public string Remote { get; set; }
        public int Size { get; set; }
        public string Content { get; set; }
    }
}
