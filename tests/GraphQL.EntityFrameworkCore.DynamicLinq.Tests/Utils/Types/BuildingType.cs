using System.Diagnostics.CodeAnalysis;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;

[ExcludeFromCodeCoverage]
public class BuildingType : ObjectGraphType<Building>
{
    public BuildingType()
    {
        Field<ListGraphType<RoomType>>("Rooms", resolve: context => context.Source.Rooms);
        Field(x => x.Id);
        Field(x => x.Name);
    }
}