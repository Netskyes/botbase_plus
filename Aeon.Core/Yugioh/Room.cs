using Aeon.Internal;
using System.Collections.Generic;

namespace Aeon.Core.Yugioh
{
    public class Room
    {
        public Room()
        {
            Users = new List<User>();
        }

        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string Desc { get; set; }
        public string NeedPass { get; set; }
        public string BanList { get; set; }
        public List<User> Users { get; set; }
        public string IStart { get; set; }

        public static List<Room> GetRooms()
        {
            var response = Utils.GetRequest<YgoRooms>("http://207.180.196.2:4707").Result;
            return response?.Rooms ?? new List<Room>();
        }
    }
}
