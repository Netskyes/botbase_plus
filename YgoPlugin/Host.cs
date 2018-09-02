using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PacketEditor.Api;

namespace YgoPlugin
{
    public class Host : Core
    {
        public void PluginRun()
        {
            ClientRecv += Host_ClientRecv;

            Log("Plugin started!");
            SendPacket(new byte[] { 0, 1, 2 });


            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private void Host_ClientRecv(byte[] bytes)
        {
            Log("Received bytes: " + bytes.Length);
        }

        public void PluginStop()
        {

        }
    }
}
