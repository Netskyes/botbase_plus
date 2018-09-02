using System;
using NetRelay.Utils;
using NetRelay.Network;
using NetRelay.Network.Objects;

namespace PacketEditor.Api
{
    public class CoreBase : PluginObject
    {
        internal ProxyHandler proxy;
        internal void SetProxy(ProxyHandler proxyHandler)
            => proxy = proxyHandler;

        internal void RegisterEventHandlers()
        {
            proxy.ClientRecv += OnClientRecv;
        }

        public CoreBase()
        {
        }

        // Call proxy methods
        public void SendPacket(byte[] bytes) => proxy.SendPacket(bytes);
        public void Log(string text) => proxy.ConsoleLog(text);

        public event ClientRecvEventHandler ClientRecv;
        public void OnClientRecv(byte[] bytes) => ClientRecv?.Invoke(bytes);
    }
}
