using GraphQL.Types;
using MyHotel.Models;

namespace MyHotel.GraphQL.Types
{
    public class BuildingType : ObjectGraphType<BuildingModel>
    {
        public BuildingType()
        {
            Field(x => x.Id);
            Field(x => x.Name);
            Field<ListGraphType<RoomType>>(nameof(BuildingModel.Rooms));
        }
    }
}
