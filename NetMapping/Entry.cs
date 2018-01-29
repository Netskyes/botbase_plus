using UnityEngine;
using AlbionBot.Plugins;

namespace UnityGameObject
{
    public class Entry
    {
        public static GameObject TeObj { get; set; }
        public static TaskExecutor TaskExecutor { get; set; }

        public static void Load()
        {
            TeObj = new GameObject();
            TeObj.AddComponent<TaskExecutor>();
            Object.DontDestroyOnLoad(TeObj);

            TaskExecutor = TeObj.GetComponent<TaskExecutor>();

            new Plugin();
        }
    }
}
