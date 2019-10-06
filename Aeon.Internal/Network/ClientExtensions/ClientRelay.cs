using System;

namespace Aeon.Internal.Network.ClientExtensions
{
    using Data;
    using Enums;

    public class ClientRelay : Client
    {
        public int ID { get; set; }
        public Client Parent { get; set; }

        public ClientRelay(Client parent)
        {
            Parent = parent;
            BUFFER_SIZE = (1024 * 1024) * 50; // 50MB
        }

        protected override void AsyncRecvProcess(Packet packet)
        {
            Parent.Send(packet);
        }

        protected override void SendProcess(Packet packet)
        {
            //IsHelloMessage(packet);

            base.SendProcess(packet);
        }

        private bool IsHelloMessage(Packet packet)
        {
            if (packet != null && packet.Buffer.Length > 8 && packet.Buffer.MatchPattern(new byte[] { 0x16, 0x03 }))
            {
                try
                {
                    var message = new byte[9];
                    int beginIndex = Array.IndexOf(packet.Buffer, 0x16);

                    Array.Copy(packet.Buffer, beginIndex, message, 0, message.Length);
                    SecurityProtocolType securityProtocolType = SecurityProtocolType.Unknown;
                    if (message[2] < 4)
                    {
                        securityProtocolType = (SecurityProtocolType)message[2];
                    }

                    Console.WriteLine("Https: " + packet.EndPoint + " SPT: " + securityProtocolType);
                    //16 03 01 02 00 01 00 01 FC
                }
                catch (Exception e)
                {
                    Utils.Log(e.Message + " " + e.StackTrace, "error");
                }
            }

            return false;
        }
    }
}
