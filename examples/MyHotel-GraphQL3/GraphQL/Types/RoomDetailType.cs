using GraphQL.Types;
using MyHotel.Models;

namespace MyHotel.GraphQL.Types
{
    public class RoomDetailType : ObjectGraphType<RoomDetailModel>
    {
        public RoomDetailType()
        {
            Field(x => x.Identifier);
            Field(x => x.Windows);
            Field(x => x.Beds);
        }
    }
}