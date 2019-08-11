using GraphQL.Types;
using MyHotel.EntityFrameworkCore.Entities;

namespace MyHotel.GraphQL.Types
{
    public class RoomStatusType : EnumerationGraphType<RoomStatus>
    {
        public RoomStatusType()
        {
            Description = "Shows if the room is available or not.";
        }
    }
}
