namespace NetRelay.Network
{
    public class Delegates
    {
        public delegate void SendPacket(byte[] bytes);
        public delegate void ConsoleLog(string text);
    }
}
