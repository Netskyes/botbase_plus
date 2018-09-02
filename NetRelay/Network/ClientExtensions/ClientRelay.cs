using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetRelay.Network.ClientExtensions
{
    using Utils;
    using Objects;

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

        protected override void AsyncProcess(Packet packet)
        {
            //Utils.Log("Received packet: " + packet.Buffer.Length);
            if (packet.Buffer.Length == 106)
            {
                Parent.Send(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            }
            else
            {
                Parent.Send(packet);
            }
        }
    }
}
