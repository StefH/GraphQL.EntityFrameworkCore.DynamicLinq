using MyHotel.EntityFrameworkCore.Entities;

namespace MyHotel.Models
{
    public class FlatRoomModel
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        public RoomStatus Status { get; set; }

        public bool AllowedSmoking { get; set; }

        public int? Windows { get; set; }

        public int? Beds { get; set; }
    }
}
