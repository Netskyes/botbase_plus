using System;
using System.Threading;
using System.Reflection;
using Aeon.Internal.Core;

namespace Aeon.Internal
{
    public sealed class PluginHandler : PluginObject
    {
        public bool IsLoaded { get; set; }
        public object CorePlugin { get; set; }
        public string PluginPath { get; set; }
        public ProxyBase CoreHandler { get; set; }

        private AppDomain appDomain;
        private Thread thread;
        private AssemblyLoader assemblyLoader;
        
        public void Launch()
        {
            appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            assemblyLoader = appDomain.CreateInstanceAndUnwrap
                (Assembly.GetExecutingAssembly().FullName, typeof(AssemblyLoader).FullName) as AssemblyLoader;

            thread = new Thread(() => 
                assemblyLoader.LoadAssembly(PluginPath, CoreHandler, this));
            thread.Start();
        }

        public void Stop()
        {
            if (IsLoaded)
            {
                try
                {
                    thread.Abort();
                    //AppDomain.Unload(appDomain);
                }
                catch (Exception e)
                {
                    Utils.Log(e.Message + " " + e.StackTrace, "error");
                }
            }
        }
    }
}
