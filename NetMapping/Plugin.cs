using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlbionBot.Api;

namespace AlbionBot.Plugins
{
    public class Plugin : Core
    {
        private Thread mainThread;

        public Plugin()
        {
            mainThread = new Thread(new ThreadStart(Loop));
            mainThread.Start();
        }

        private void Loop()
        {
            while (true)
            {
                GameTask(() => Player.Move(new Vector3(244.3613f, 0, -30.63479f)));
                Thread.Sleep(2000);
                GameTask(() => Player.Move(new Vector3(249.2515f, 0, -30.677f)));

                var pos = Player.GetPosition();

                Log("My pos: " + pos.x + " " + pos.y + " " + pos.z);
            }
        }
    }
}
