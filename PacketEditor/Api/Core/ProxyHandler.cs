using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetRelay.Network.Delegates;

namespace PacketEditor.Api
{
    internal class ProxyHandler : MarshalByRefObject
    {
        // Delegates
        public SendPacket SendPacketProxy { get; set; }
        public void SendPacket(byte[] bytes) => SendPacketProxy(bytes);

        public ConsoleLog ConsoleLogProxy { get; set; }
        public void ConsoleLog(string text) => ConsoleLogProxy(text);
        
        public ProxyHandler()
        {
        }
    }
}
