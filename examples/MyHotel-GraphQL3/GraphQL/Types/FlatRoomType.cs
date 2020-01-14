using GraphQL.Types;
using MyHotel.Models;

namespace MyHotel.GraphQL.Types
{
    public class FlatRoomType : ObjectGraphType<FlatRoomModel>
    {
        public FlatRoomType()
        {
            Field(m => m.Id);
            Field(m => m.Name);
            Field(m => m.Number);
            Field(x => x.AllowedSmoking);
            Field<RoomStatusType>(nameof(RoomModel.Status));

            // https://bartwullems.blogspot.com/2018/11/graphqldotnetnullable-types-nullable.html
            Field(m => m.Windows, true);
            Field(m => m.Beds, true);
        }
    }
}