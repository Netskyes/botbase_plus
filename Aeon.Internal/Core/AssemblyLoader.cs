using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Aeon.Internal.Core
{
    internal class AssemblyLoader : PluginObject
    { 
        private object pluginObj;
        private MethodInfo pluginStop;

        [HandleProcessCorruptedStateExceptions]
        public bool LoadAssembly(string path, ProxyBase coreHandler, PluginHandler pluginHandler)
        {
            var assembly = Assembly.LoadFile(path);
            var types = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(CoreBase))).ToArray();
            
            for (int i = 0; i < types.Length; i++)
            {
                FieldInfo coreBase = types[i].GetField
                    ("proxyBase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo pluginRun = types[i].GetMethod("PluginRun");
                pluginStop = types[i].GetMethod("PluginStop");

                if (pluginRun != null)
                {
                    pluginObj = assembly.CreateInstance(types[i].FullName);
                    coreBase.SetValue(pluginObj, coreHandler);
                    pluginHandler.IsLoaded = true;
                    pluginHandler.CorePlugin = pluginObj;

                    if (!coreHandler.LaunchedPlugins.Contains(pluginHandler))
                    {
                        coreHandler.LaunchedPlugins.Add(pluginHandler);
                    }

                    pluginRun.Invoke(pluginObj, null);

                    return true;
                }
            }

            return false;
        }

        [HandleProcessCorruptedStateExceptions]
        public void StopAssembly()
        {
            try
            {
                if (pluginStop != null)
                {
                    pluginStop.Invoke(pluginObj, null);
                }
            }
            catch (Exception e)
            {
                Utils.Log(e.Message + " " + e.StackTrace, "error");
            }
        }
    }
}
