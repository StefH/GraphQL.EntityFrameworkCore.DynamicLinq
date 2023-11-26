using System.Diagnostics.CodeAnalysis;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;

[ExcludeFromCodeCoverage]
public class RoomType : ObjectGraphType<Room>
{
    public RoomType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
        Field(x => x.Number);
        Field(x => x.AllowedSmoking);
        Field<RoomStatusType>(nameof(Room.Status));
        Field<RoomDetailType>(nameof(Room.RoomDetail));
    }
}