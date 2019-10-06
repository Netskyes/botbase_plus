using System;

namespace Aeon.Internal.Core
{
    public class CoreBase : PluginObject
    {
        public CoreBase()
        {
        }

        public void Log(string text)
        {
            if (!string.IsNullOrEmpty(text)) proxyBase.Log(text);
        }

        public void Log(string text, string tabName)
        {
            if (!string.IsNullOrEmpty(text)) proxyBase.Log(text, tabName);
        }

        public void DebugLog(string text)
        {
            Utils.Log(text, "debug");
        }

        public bool InjectX86(string processName)
        {
            return Externals.Inject(processName, Utils.AssemblyDirectory + @"\InjX86.dll");
        }

        public ProxyBase proxyBase;
    }
}
