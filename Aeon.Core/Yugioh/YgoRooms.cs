using System.Collections.Generic;

namespace Aeon.Core.Yugioh
{
    public class YgoRooms
    {
        public YgoRooms()
        {
            Rooms = new List<Room>();
        }

        public List<Room> Rooms { get; set; }
    }
}
