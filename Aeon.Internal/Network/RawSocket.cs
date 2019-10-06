using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Aeon.Internal.Network
{
    public class RawSocket
    {
        private Socket socket;
        private byte[] buffer = new byte[4096];

        public void Begin(short port)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
                socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
                byte[] byInV = { 1, 0, 0, 0 };
                byte[] byOut = { 1, 0, 0, 0 };
                socket.IOControl(IOControlCode.ReceiveAll, byInV, byOut);

                socket.BeginReceive
                    (buffer, 0, buffer.Length, SocketFlags.None, AsyncRecv, null);
            }
            catch (Exception e)
            {
                Utils.Log(e.Message + " " + e.StackTrace, "error");
            }
        }

        protected virtual void AsyncRecv(IAsyncResult result)
        {
            int recvSize = socket.EndReceive(result);
        }

        private void Test()
        {

        }
    }
}
