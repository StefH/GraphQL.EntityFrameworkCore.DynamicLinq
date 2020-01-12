using GraphQL.Types;
using MyHotel.Models;

namespace MyHotel.GraphQL.Types
{
    public class RoomType : ObjectGraphType<RoomModel>
    {
        public RoomType()
        {
            Field(x => x.Id);
            Field(x => x.Name);
            Field(x => x.Number);
            Field(x => x.AllowedSmoking);
            Field<RoomStatusType>(nameof(RoomModel.Status));
            Field<RoomDetailType>(nameof(RoomModel.Detail));
            Field<BuildingType>(nameof(RoomModel.Building));
        }
    }
}
