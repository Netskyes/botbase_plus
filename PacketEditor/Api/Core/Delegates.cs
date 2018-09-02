namespace PacketEditor.Api
{
    public delegate void SendPacket(byte[] bytes);
    public delegate void ConsoleLog(string text);
    public delegate void ClientRecvEventHandler(byte[] bytes);
}
