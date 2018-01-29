using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityGameObject;

namespace AlbionBot.Api
{
    public class Core
    {
        public Core()
        {
        }

        public static void GameTask(Action action)
            => Entry.TaskExecutor.ScheduleTask(action);

        public static T GetUnityObject<T>() where T : UnityEngine.Object
            => UnityEngine.Object.FindObjectOfType<T>();

        public static void Log(string text)
            => File.AppendAllText("C:\\albion_debug.txt", text + Environment.NewLine);
    }
}
