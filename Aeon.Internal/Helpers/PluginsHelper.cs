using System;
using System.IO;
using System.Collections.Generic;
using Aeon.Internal.Core;
using System.Security.Policy;

namespace Aeon.Internal
{
    internal sealed class PluginsHelper
    {
        private List<string> pluginsList;
        private AppDomain appDomain;
        private AppDomainSetup domainSetup;
        private PluginsValidator pluginValidator;

        public PluginsHelper()
        {
            pluginsList = new List<string>();
        }

        public List<string> GetPlugins() => (pluginsList = FetchPlugins());

        private void CreateSandbox()
        {
            domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = Utils.AssemblyDirectory + @"\";
            Evidence evidence = AppDomain.CurrentDomain.Evidence;
            //PermissionSet perms = new PermissionSet(System.Security.Permissions.PermissionState.None);
            Type pvType = typeof(PluginsValidator);

            appDomain = AppDomain.CreateDomain("sandbox", evidence, domainSetup);
            pluginValidator = (PluginsValidator)appDomain.CreateInstanceAndUnwrap(pvType.Assembly.FullName, pvType.FullName);
        }

        private bool IsPluginValid(string path)
        {
            try
            {
                return pluginValidator.IsPluginValid(path, typeof(CoreBase));
            }
            catch (Exception e)
            {
                Utils.Log(e.Message + " " + e.StackTrace, "error");
            }

            return false;
        }

        internal List<string> FetchPlugins()
        {
            var tempPlugins = new List<string>();

            try
            {
                CreateSandbox();

                var array = Directory.GetFiles(Utils.AssemblyDirectory + @"\Plugins", "*.dll", SearchOption.AllDirectories);
                for (int i = 0; i < array.Length; i++)
                {
                    if (IsPluginValid(array[i]) && !pluginsList.Contains(array[i]))
                    {
                        tempPlugins.Add(array[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Log(e.Message + " " + e.StackTrace, "error");
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }

            return tempPlugins;
        }
    }
}
