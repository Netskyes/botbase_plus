using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetRelay.Network
{
    public class ClientRelay : Client
    {
        public int ID { get; set; }
        public Client Parent { get; set; }

        public ClientRelay(Client parent)
        {
            Parent = parent;
            ID = new Random().Next();
            BUFFER_SIZE = (1024 * 1024) * 50; // 50MB
        }

        protected override void OnClientRecv(Packet packet)
        {
            Parent.Send(packet);
            base.OnClientRecv(packet);
        }
    }
}
