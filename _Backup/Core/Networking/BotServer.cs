using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotRelay.Core.Networking
{
    class BotServer : Server
    {
        public BotServer() : base()
        {

        }

        public Client[] ConnectedClients
        {
            get
            {
                return Clients.Where(c => c != null).ToArray();
            }
        }
    }
}
