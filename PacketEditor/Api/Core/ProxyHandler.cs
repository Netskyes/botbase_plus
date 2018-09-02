using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetRelay.Network;
using NetRelay.Network.Objects;

namespace PacketEditor.Api
{
    internal class ProxyHandler : MarshalByRefObject
    {
        // Delegates
        public SendPacket SendPacketProxy { get; set; }
        public void SendPacket(byte[] bytes) => SendPacketProxy(bytes);

        public ConsoleLog ConsoleLogProxy { get; set; }
        public void ConsoleLog(string text) => ConsoleLogProxy(text);

        public event ClientRecvEventHandler ClientRecv;
        public void OnClientRecv(byte[] bytes) => ClientRecv?.Invoke(bytes);

        public ProxyHandler()
        {
            
        }
    }
}
