using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Aeon.Internal
{
    public static class Utils
    {
        public static void Log(string text, string outputName = "debug")
        {
            try
            {
                File.AppendAllText(AssemblyDirectory + @"\" + outputName + ".log", text + Environment.NewLine);
            }
            catch
            {
            }
        }

        public static void LogException(Exception e)
        {
            Log(e.Message + " " + e.StackTrace, "error"); // Format exception properly
        }

        public static string AssemblyDirectory
        {
            get
            {
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }

        public static bool MatchPattern(this byte[] array, byte[] pattern)
        {
            if (array is null || array.Length < 1 
                || pattern is null || pattern.Length < 1)
                return false;

            for (int i = 0; i < pattern.Length; i++)
            {
                if (i < array.Length && array[i] != pattern[i])
                    return false;
            }

            return true;
        }

        public static void Invoke(Action action)
        {
            Dispatcher.CurrentDispatcher.Invoke(action);
        }

        public static T Invoke<T>(Func<T> action)
        {
            T result = default(T);
            Dispatcher.CurrentDispatcher.Invoke(() => result = action.Invoke());
            return result;
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static byte[] HexToByteArrayNoSpaces(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] HexToByteArray(string hex)
        {
            var hexStrings = hex.Split(new string[] { " " }, StringSplitOptions.None);
            byte[] bytes = new byte[hexStrings.Length];
            for (int i = 0; i < hexStrings.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexStrings[i], 16);
            }

            return bytes;
        }

        public static async Task<T> GetRequest<T>(string requestUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(requestUri);
                    var responseString = await response.Content.ReadAsStringAsync();

                    return Serializer.ToJsonObject<T>(responseString);
                }
            }
            catch
            {
                return default(T);
            }
        }

        public static async Task<T> PostRequest<T>(string requestUri, HttpContent content)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = await client.PostAsync(requestUri, content);
                    var responseString = await request.Content.ReadAsStringAsync();

                    return Serializer.ToJsonObject<T>(responseString);
                }
            }
            catch
            {
                return default(T);
            }
        }
    }
}
