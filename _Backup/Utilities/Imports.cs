using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace BotRelay
{
    public static class Imports
    {
        [DllImport("G:/SoftwareDev/C#/BotRelay/Debug/Tools.dll")]
        public extern static bool Inject(string process, string dll);
    }
}
