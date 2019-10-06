using System.Runtime.InteropServices;

namespace Aeon.Internal
{
    internal static class Externals
    {
        [DllImport("Aeon.Tools.dll")]
        public extern static bool Inject(string procName, string dll);
    }
}
