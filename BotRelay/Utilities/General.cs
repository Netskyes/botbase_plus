using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BotRelay
{
    public static class Utils
    {
        public static void InvokeOn(Control ctl, Action action)
        {
            if (ctl != null)
            {
                if (ctl.InvokeRequired)
                {
                    ctl.Invoke(new Action(() => InvokeOn(ctl, action)));
                }
                else action();
            }
        }

        public static void DebugLog(string text)
        {
            string path = "C:/Users/SumYungHo/Desktop/Debug.txt";

            try
            {
                File.AppendAllText(path, text + Environment.NewLine);
            }
            catch
            {
            }
        }

        public static void Delay(int ms, CancellationToken token)
        {
            Task.Delay(ms, token).Wait();
        }

        public static void Delay(int min, int max, CancellationToken token)
        {
            Task.Delay(RandomNum(min, max), token).Wait();
        }

        public static void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        public static void Sleep(int minms, int maxms)
        {
            Random rand = new Random();
            int ms = rand.Next(minms, maxms);

            Thread.Sleep(ms);
        }

        public static int RandomNum()
        {
            int ms = DateTime.Now.Millisecond;
            int rand = ms - RandomNum(0, (ms / 2));

            return rand;
        }

        public static int RandomNum(int min, int max)
        {
            Random rand = new Random();
            return rand.Next(min, max);
        }

        public static double RandomDouble(double min, double max)
        {
            Random rand = new Random();
            return rand.NextDouble() * (min - max) + max;
        }

        public static T[] RandomPermutation<T>(T[] array)
        {
            T[] retArray = new T[array.Length];
            array.CopyTo(retArray, 0);

            Random random = new Random();
            for (int i = 0; i < array.Length; i += 1)
            {
                int swapIndex = random.Next(i, array.Length);
                if (swapIndex != i)
                {
                    T temp = retArray[i];
                    retArray[i] = retArray[swapIndex];
                    retArray[swapIndex] = temp;
                }
            }

            return retArray;
        }
    }
}
