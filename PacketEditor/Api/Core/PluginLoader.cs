using System;
using System.Linq;
using System.Reflection;
using NetRelay.Utils;
using NetRelay.Network;

namespace PacketEditor.Api
{
    class PluginLoader : MarshalByRefObject
    {
        internal void Load(string pluginPath, ProxyHandler proxy)
        {
            var asm = Assembly.LoadFrom(pluginPath);
            var pluginEntry = asm?.GetTypes().FirstOrDefault
                (x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Core)));
            if (pluginEntry is null)
                return;

            var instance = AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(asm.Location, pluginEntry.FullName);
            (instance as Core).SetProxy(proxy);

            var plugin = pluginEntry.GetMethods().FirstOrDefault(x => x.Name == "PluginRun");
            if (plugin != null)
            {
                try
                {
                    instance.GetType().GetMethod("PluginRun").Invoke(instance, null);
                }
                catch (Exception e)
                {
                    Utils.Log(e.Message + " " + e.StackTrace);
                }
            }
        }
    }
}
