using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PacketEditor.Api;

namespace YgoPlugin
{
    public class Host : Core
    {
        public void PluginRun()
        {
            Log("Plugin started!");
            SendPacket(new byte[] { 0, 1, 2 });
        }

        public void PluginStop()
        {

        }
    }
}
