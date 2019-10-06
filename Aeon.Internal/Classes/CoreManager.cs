using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeon.Internal
{
    public sealed class CoreManager
    {
        private List<ProxyBase> cores;

        internal CoreManager()
        {
            cores = new List<ProxyBase>();
        }

        public void AddCore(ProxyBase core)
        {
        }

        public void RemoveCore(ProxyBase core)
        {
        }

        public List<ProxyBase> GetCores()
        {
            return cores;
        }
    }
}
