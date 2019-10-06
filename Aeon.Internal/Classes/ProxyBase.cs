using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aeon.Internal;
using Aeon.Internal.Core;

namespace Aeon.Internal
{
    public class ProxyBase : PluginObject
    {
        protected internal List<PluginHandler> LaunchedPlugins { get; set; } = new List<PluginHandler>();

        public ProxyBase()
        {
        }

        protected internal void LaunchPlugin(string asmPath)
        {
            var ph = new PluginHandler();
            ph.PluginPath = asmPath;
            ph.CoreHandler = this;
            ph.Launch();
        }

        public void Log(string text, string tabName = "Main")
        {
            AppInternal.MainWindow.Log(text, tabName);
        }
    }
}
