using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using Aeon.Core;
using Aeon.Core.Yugioh;

namespace Plugin
{
    public class Host : Core
    {
        public readonly string[] HookProcs = { "ygopro_vs_links.exe" };

        public void PluginRun()
        {
            Log("Plugin started!");

            bool isHooked = false;

            while (!isHooked)
            {
                for (int i = 0; i < HookProcs.Length; i++)
                {
                    if (InjectX86(HookProcs[i]))
                    {
                        Log("Hooked to: " + HookProcs[i]);
                        isHooked = true;
                        break;
                    }
                }

                Thread.Sleep(100);
            }

            var test = Room.GetRooms();
            Log("Room numbers: " + test.Count);

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        public void PluginStop()
        {
            Log("Plugin stopped!");
        }
    }
}
