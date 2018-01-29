using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameObject
{
    public class TaskExecutor : MonoBehaviour
    {
        public bool EnableGUI { get; set; }

        private Queue<Action> taskQueue = new Queue<Action>();
        private readonly object queueLock = new object();

        public void ScheduleTask(Action action)
        {
            if (action == null)
                return;

            lock (queueLock)
            {
                taskQueue.Enqueue(action);
            }
        }

        void Start()
        {
            EnableGUI = true;
        }

        void Update()
        {
            lock (queueLock)
            {
                if (taskQueue.Count > 0)
                {
                    taskQueue.Dequeue().Invoke();
                }
            }
        }

        void OnGUI()
        {
            if (!EnableGUI)
                return;

            GUI.color = Color.red;
            GUI.Label(new Rect(50, 50, 200, 40), "Executor Active");
        }
    }
}
