using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BotRelay.Core.Networking
{
    public class UdpBotServer
    {
        private Socket handle;

        private byte[] readBuffer;


        public UdpBotServer()
        {
            readBuffer = new byte[1024 * 1024]; // 1MB
        }

        public void Listen(ushort port)
        {
            try
            {
                handle = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                handle.Bind(new IPEndPoint(IPAddress.Any, port));

                handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);

            }
            catch (SocketException)
            {

            }
            catch (Exception)
            {

            }
        }

        private void AsyncReceive(IAsyncResult result)
        {
            try
            {
                int bytesTransfered = handle.EndReceive(result);

                System.Windows.Forms.MessageBox.Show("Trans: " + bytesTransfered);
            }
            catch
            {

            }



            try
            {
                handle.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, AsyncReceive, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {

            }
        }
    }
}
