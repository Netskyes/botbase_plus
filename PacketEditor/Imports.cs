using System.Runtime.InteropServices;

namespace PacketEditor
{
    public static class Imports
    {
        [DllImport("G:/SoftwareDev/C#/BotRelay/Debug/Tools.dll")]
        public extern static bool Inject(string procName, string dll);
    }
}
