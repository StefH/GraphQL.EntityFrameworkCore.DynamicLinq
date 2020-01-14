using System.Collections.Generic;

namespace MyHotel.Models
{
    public class BuildingModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<RoomModel> Rooms { get; set; }
    }
}