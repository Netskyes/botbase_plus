using System;
using NetRelay.Utils;
using NetRelay.Network;
using static NetRelay.Network.Delegates;

namespace PacketEditor.Api
{
    public class CoreBase : PluginObject
    {
        internal ProxyHandler proxy;
        internal void SetProxy(ProxyHandler proxyHandler)
            => proxy = proxyHandler;

        public CoreBase()
        {
        }

        public void SendPacket(byte[] bytes) => proxy.SendPacket(bytes);
        public void Log(string text) => proxy.ConsoleLog(text);
    }
}
