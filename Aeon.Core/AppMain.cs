using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aeon.Internal;

namespace Aeon
{
    internal static class AppMain
    {
        [STAThread]
        private static void Main(string[] args)
        {
            AppInternal.Init();
        }
    }
}
