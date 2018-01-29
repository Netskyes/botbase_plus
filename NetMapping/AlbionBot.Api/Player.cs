using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AlbionBot.Api
{
    public class Player : Core
    {
        private static LocalPlayerCharacterView GetLocalPlayer() => GetUnityObject<LocalPlayerCharacterView>();
        private static PlayerCharacterView GetPlayer() => GetUnityObject<PlayerCharacterView>();

        public static bool Move(Vector3 position)
        {
            var player = GetLocalPlayer();

            return player != null && position != null
                && player.RequestMove(new amn(position.x, position.z));
        }

        public static Vector3 GetPosition()
        {
            var player = GetLocalPlayer();
            
            return (player != null) 
                ? player.GetPosition() : Vector3.zero;
        }
    }
}
