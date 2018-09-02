using System;
using System.IO;
using System.Reflection;

namespace NetRelay.Utils
{
    public static class Utils
    {
        public static void Log(string text)
        {
            try
            {
                File.AppendAllText(AssemblyDirectory + @"\PacketEditor.log", text + Environment.NewLine);
            }
            catch
            {
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }
    }
}
